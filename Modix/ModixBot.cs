using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService( new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug
        });
        private DiscordSocketClient _client;
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly ModixBotHooks _hooks = new ModixBotHooks();
        private ModixConfig _config = new ModixConfig();

        public ModixBot()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(@"logs\{Date}")
                .CreateLogger();
        }

        public async Task Run()
        {
            LoadConfig();
            _client = new DiscordSocketClient(config: new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
            });

            await Install(); // Setting up DependencyMap
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
                PostgreConnectionString = Environment.GetEnvironmentVariable("PostgreConnectionString")
            };
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
            
            stopwatch.Stop();
            Log.Information($"Took {stopwatch.ElapsedMilliseconds}ms to process: {message}");
        }

        public async Task Install()
        {
            _map.AddSingleton(_client);
            _map.AddSingleton(_config);

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += _hooks.HandleMessage;
            _client.Log += _hooks.HandleLog;
            _commands.Log += _hooks.HandleLog;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
