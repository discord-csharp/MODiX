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
using Modix.Utilities;
using Serilog.Events;
using Modix.Data;
using Modix.Services.Cat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug
        });
        private DiscordSocketClient _client;
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly ModixBotHooks _hooks = new ModixBotHooks();
        private ModixConfig _config = new ModixConfig();

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
            _map.AddDbContext<ModixContext>(options =>
            {
                options.UseNpgsql(_config.PostgreConnectionString);                
            });
           
            var provider = _map.BuildServiceProvider();

            var loggerFactory = provider.GetService<ILoggerFactory>();
            using (var context = provider.GetService<ModixContext>())
            {
                context.Database.Migrate();
            }
            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        public void LoadConfig()
        {

            _config = new ModixConfig
            {
                DiscordToken = Environment.GetEnvironmentVariable("Token"),
                ReplToken = Environment.GetEnvironmentVariable("ReplToken"),
                StackoverflowToken = Environment.GetEnvironmentVariable("StackoverflowToken"),
                PostgreConnectionString = Environment.GetEnvironmentVariable("MODIX_DB_CONNECTION"),
            };
            var id = Environment.GetEnvironmentVariable("log_webhook_id");

            if (string.IsNullOrWhiteSpace(id))
            {
                _config.WebhookId = ulong.Parse(id);
                _config.WebhookToken = Environment.GetEnvironmentVariable("log_webhook_token");
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

            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _map.BuildServiceProvider());

            if (!result.IsSuccess)
            {
                Log.Error($"{result.Error}: {result.ErrorReason}");
            }

            stopwatch.Stop();
            Log.Information($"Took {stopwatch.ElapsedMilliseconds}ms to process: {message}");
        }

        public async Task Install()
        {
            _map.AddSingleton(_client);
            _map.AddSingleton(_config);
            _map.AddSingleton<ICatService, CatService>();
            _map.AddScoped<IQuoteService, QuoteService>();

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += _hooks.HandleMessage;
            _client.ReactionAdded += _hooks.HandleAddReaction;
            _client.ReactionRemoved += _hooks.HandleRemoveReaction;

            _client.Log += _hooks.HandleLog;
            _commands.Log += _hooks.HandleLog;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
