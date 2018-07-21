﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services;
using Modix.Services.AutoCodePaste;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.GuildInfo;
using Modix.Services.Moderation;
using Modix.Services.Quote;
using Modix.WebServer;
using Serilog;
using Modix.Data.Repositories;
using Modix.Handlers;
using Modix.Services.BehaviourConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Services.FileUpload;
using Modix.Services.Notes;
using Modix.Services.Promotions;

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

        public ModixBot(ModixConfig config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _map.AddLogging(bldr => bldr.AddSerilog(logger ?? Log.Logger));
        }

        public async Task Run()
        {
            _client = new DiscordSocketClient(config: new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
            });

            await Install(); // Setting up DependencyMap

            _map.AddDbContext<ModixContext>(options =>
            {
                options.UseNpgsql(_config.PostgreConnectionString);
            }, ServiceLifetime.Transient);

            _host = ModixWebServer.BuildWebHost(_map, _config);

            _scope = _host.Services.CreateScope();

            using (var context = _scope.ServiceProvider.GetService<ModixContext>())
            {
                context.Database.Migrate();
            }

            _hooks.ServiceProvider = _scope.ServiceProvider;
            foreach (var behavior in _scope.ServiceProvider.GetServices<IBehavior>())
                await behavior.StartAsync();


            var configurationService = _scope.ServiceProvider.GetRequiredService<IBehaviourConfigurationService>();

            // Cache the behaviour configuration
            await configurationService.LoadBehaviourConfiguration();

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
            Task.Run(async () =>
            {
                var context = new CommandContext(_client, message);

                using (var scope = _scope.ServiceProvider.CreateScope())
                {
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
            await Task.CompletedTask;
        }

        public async Task Install()
        {
            _map.AddSingleton(_client);
            _map.AddSingleton<IDiscordClient>(_client);
            _map.AddSingleton(_config);
            _map.AddSingleton(_commands);

            _map.AddModixCore()
                .AddModixModeration();

            _map.AddScoped<IQuoteService, QuoteService>();
            _map.AddSingleton<CodePasteHandler>();
            _map.AddSingleton<FileUploadHandler>();
            _map.AddSingleton<CodePasteService>();
            _map.AddMemoryCache();

            _map.AddSingleton<GuildInfoService>();
            _map.AddSingleton<ICodePasteRepository, MemoryCodePasteRepository>();
            _map.AddSingleton<CommandHelpService>();

            _map.AddSingleton<PromotionService>();
            _map.AddSingleton<IPromotionRepository, DBPromotionRepository>();

            _map.AddSingleton<CommandErrorHandler>();
            _map.AddSingleton<InviteLinkHandler>();
            _map.AddScoped<IBehaviourConfigurationRepository, BehaviourConfigurationRepository>();
            _map.AddScoped<IBehaviourConfigurationService, BehaviourConfigurationService>();
            _map.AddSingleton<IBehaviourConfiguration, BehaviourConfiguration>();

            _map.AddSingleton<INoteCreatorService, NoteCreatorService>();
            _map.AddSingleton<INoteRetrieverService, NoteRetrieverService>();
            _map.AddSingleton<INoteRemoverService, NoteRemoverService>();

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += _hooks.HandleMessage;
            _client.ReactionAdded += _hooks.HandleAddReaction;
            _client.ReactionRemoved += _hooks.HandleRemoveReaction;
            _client.UserJoined += _hooks.HandleUserJoined;
            _client.UserLeft += _hooks.HandleUserLeft;

            _client.Log += _hooks.HandleLog;
            _commands.Log += _hooks.HandleLog;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
