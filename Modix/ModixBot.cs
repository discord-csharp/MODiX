using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modix.Behaviors;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services;
using Modix.Services.AutoCodePaste;
using Modix.Services.BehaviourConfiguration;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.DocsMaster;
using Modix.Services.GuildInfo;
using Modix.Services.Moderation;
using Modix.Services.PopularityContest;
using Modix.Services.Promotions;
using Modix.Services.Quote;
using Modix.WebServer;
using Serilog;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,
            DefaultRunMode = RunMode.Sync,
            CaseSensitiveCommands = false,
            SeparatorChar = ' '
        });

        private DiscordSocketClient _client;
        private readonly IServiceCollection _map = new ServiceCollection();
        private IServiceScope _scope;
        private readonly ModixBotHooks _hooks = new ModixBotHooks();
        private readonly ModixConfig _config;
        private IWebHost _host;

        public ModixBot(ModixConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _map.AddLogging(bldr => bldr.AddSerilog());
        }

        public async Task Run()
        {
            _client = new DiscordSocketClient(config: new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = _config.MessageCacheSize //needed to log deletions
            });

            await Install(); // Setting up DependencyMap

            _map.AddDbContext<ModixContext>(options =>
            {
                options.UseNpgsql(_config.PostgreConnectionString);
            });

            _host = ModixWebServer.BuildWebHost(_map, _config);

            _scope = _host.Services.CreateScope();

            using (var scope = _scope.ServiceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ModixContext>()
                    .Database.Migrate();

                await scope.ServiceProvider.GetRequiredService<IBehaviourConfigurationService>()
                    .LoadBehaviourConfiguration();
            }

            _hooks.ServiceProvider = _scope.ServiceProvider;
            foreach (var behavior in _scope.ServiceProvider.GetServices<IBehavior>())
                await behavior.StartAsync();

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();

            _client.Ready += StartWebserver;

            await Task.Delay(-1);
        }

        public async Task StartWebserver()
        {
            await _client.SetGameAsync("https://mod.gg/");

            //Start the webserver, but unbind the event in case discord.net reconnects
            await _host.StartAsync();
            _client.Ready -= StartWebserver;
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            if (message.Content.Length <= 1)
                return;

            // because RunMode.Async will cause an object disposed exception due to an implementation bug in discord.net. All commands must be RunMode.Sync.
#pragma warning disable CS4014
            Task.Run(async () =>
            {
                var context = new CommandContext(_client, message);

                if (!(context.User is IGuildUser)){ return; }

                using (var scope = _scope.ServiceProvider.CreateScope())
                {
                    await scope.ServiceProvider
                        .GetRequiredService<IAuthorizationService>()
                        .OnAuthenticatedAsync(context.User as IGuildUser);

                    var result = await _commands.ExecuteAsync(context, argPos, scope.ServiceProvider);

                    if (!result.IsSuccess)
                    {
                        string error = $"{result.Error}: {result.ErrorReason}";

                        if (!string.Equals(result.ErrorReason, "UnknownCommand", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Warning(error);
                        }
                        else
                        {
                            Log.Error(error);
                        }

                        if (result.Error != CommandError.Exception)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService<CommandErrorHandler>();
                            await handler.AssociateError(message, error);
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync("Error: " + error);
                        }
                    }
                }

                stopwatch.Stop();
                Log.Information($"Took {stopwatch.ElapsedMilliseconds}ms to process: {message}");
            });
#pragma warning restore CS4014

            await Task.CompletedTask;
        }

        public async Task Install()
        {
            _map.AddSingleton(_client);
            _map.AddSingleton<IDiscordClient>(_client);
            _map.AddSingleton(_config);
            _map.AddSingleton(_commands);
            _map.AddSingleton<HttpClient>();

            _map.AddModixCore()
                .AddModixModeration()
                .AddModixPromotions();

            _map.AddScoped<IQuoteService, QuoteService>();
            _map.AddSingleton<CodePasteHandler>();
            _map.AddSingleton<IBehavior, AttachmentBlacklistBehavior>();
            _map.AddSingleton<CodePasteService>();
            _map.AddScoped<DocsMasterRetrievalService>();
            _map.AddMemoryCache();

            _map.AddSingleton<GuildInfoService>();
            _map.AddSingleton<ICodePasteRepository, MemoryCodePasteRepository>();
            _map.AddSingleton<CommandHelpService>();
            _map.AddScoped<IPopularityContestService, PopularityContestService>();

            _map.AddSingleton<CommandErrorHandler>();
            _map.AddScoped<IBehaviourConfigurationRepository, BehaviourConfigurationRepository>();
            _map.AddScoped<IBehaviourConfigurationService, BehaviourConfigurationService>();
            _map.AddSingleton<IBehaviourConfiguration, BehaviourConfiguration>();

            _map.AddScoped<IModerationActionEventHandler, ModerationLoggingBehavior>()
                .AddScoped<IPromotionActionEventHandler, PromotionLoggingBehavior>();

            _client.MessageReceived += HandleCommand;
            _client.ReactionAdded += _hooks.HandleAddReaction;
            _client.ReactionRemoved += _hooks.HandleRemoveReaction;
            _client.UserJoined += _hooks.HandleUserJoined;
            _client.UserLeft += _hooks.HandleUserLeft;

            _client.Log += _hooks.HandleLog;
            _commands.Log += _hooks.HandleLog;

            _commands.AddTypeReader<IEmote>(new EmoteTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
