using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Quote;
using Serilog.Events;
using Modix.Services.AutoCodePaste;
using Modix.WebServer;
using Modix.Services.GuildInfo;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;

namespace Modix
{
    using Microsoft.AspNetCore.Hosting;
    using Services.Animals;
    using Services.FileUpload;
    using Services.Promotions;
    using Services.Utilities;

    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug
        });

        private DiscordSocketClient _client;
        private readonly IServiceCollection _map = new ServiceCollection();
        private IServiceProvider _provider;
        private ModixBotHooks _hooks = new ModixBotHooks();
        private ModixConfig _config = new ModixConfig();
        private IWebHost _host;

        public ModixBot()
        {
            LoadConfig();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug);

            if (!string.IsNullOrWhiteSpace(_config.WebhookToken))
            {
                loggerConfig.WriteTo.DiscordWebhookSink(_config.WebhookId, _config.WebhookToken, LogEventLevel.Error);
            }

            if(!string.IsNullOrWhiteSpace(_config.SentryToken))
            {
                loggerConfig.WriteTo.Sentry(_config.SentryToken, restrictedToMinimumLevel: LogEventLevel.Warning);
            }

            Log.Logger = loggerConfig.CreateLogger();
            _map.AddLogging(bldr => bldr.AddSerilog(Log.Logger, true));
        }

        public async Task Run()
        {
            _client = new DiscordSocketClient(config: new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
            });

            await Install(); // Setting up DependencyMap
            //_map.AddDbContext<ModixContext>(options =>
            //{
            //    options.UseNpgsql(_config.PostgreConnectionString);
            //});

            //var provider = _map.BuildServiceProvider();

            _host = ModixWebServer.BuildWebHost(_map, _config);

            //provider.GetService<ILoggerFactory>();

            //disable until we migrate to Xero's host.
            //#if !DEBUG

            //using (var context = provider.GetService<ModixContext>())
            //{
            //    context.Database.Migrate();
            //}

            //#endif

            _provider = _host.Services;

            _hooks.ServiceProvider = _provider;

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

        public void LoadConfig()
        {
            _config = new ModixConfig
            {
                DiscordToken = Environment.GetEnvironmentVariable("Token"),
                ReplToken = Environment.GetEnvironmentVariable("ReplToken"),
                StackoverflowToken = Environment.GetEnvironmentVariable("StackoverflowToken"),
                PostgreConnectionString = Environment.GetEnvironmentVariable("MODIX_DB_CONNECTION"),
                DiscordClientId = Environment.GetEnvironmentVariable("DiscordClientId"),
                DiscordClientSecret = Environment.GetEnvironmentVariable("DiscordClientSecret"),
            };

            var id = Environment.GetEnvironmentVariable("log_webhook_id");

            if (!string.IsNullOrWhiteSpace(id))
            {
                _config.WebhookId = ulong.Parse(id);
                _config.WebhookToken = Environment.GetEnvironmentVariable("log_webhook_token");
            }

            var sentryToken = Environment.GetEnvironmentVariable("SentryToken");
            if (!string.IsNullOrWhiteSpace(sentryToken))
            {
                _config.SentryToken = sentryToken;
            }
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

            var context = new CommandContext(_client, message);

            using (var scope = _provider.CreateScope())
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
        }

        public async Task Install()
        {
            _map.AddSingleton(_client);
            _map.AddSingleton(_config);
            _map.AddSingleton(_commands);

            _map.AddScoped<IQuoteService, QuoteService>();
            _map.AddSingleton<CodePasteHandler>();
            _map.AddSingleton<FileUploadHandler>();
            _map.AddSingleton<CodePasteService>();
            _map.AddSingleton<IAnimalService, AnimalService>();
            _map.AddMemoryCache();

            _map.AddSingleton<GuildInfoService>();
            _map.AddSingleton<ICodePasteRepository, MemoryCodePasteRepository>();
            _map.AddSingleton<CommandHelpService>();

            _map.AddSingleton<PromotionService>();
            _map.AddSingleton<IPromotionRepository, DBPromotionRepository>();

            _map.AddSingleton<CommandErrorHandler>();

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
