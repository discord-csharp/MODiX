using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService();
        private DiscordSocketClient _client;
        private readonly DependencyMap _map = new DependencyMap();
        private readonly ModixBotHooks _hooks = new ModixBotHooks();

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
            var token = Environment.GetEnvironmentVariable("MODIX_BOT_KEY");
            _client = new DiscordSocketClient(config: new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
            });

            await Install(); // Setting up DependencyMap

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
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
            var result = await _commands.ExecuteAsync(context, argPos, _map);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            stopwatch.Stop();
            Log.Information($"Took {stopwatch.ElapsedMilliseconds}ms to process: {message}");
        }

        public async Task Install()
        {
            _map.Add(_client);

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += _hooks.HandleMessage;
            _client.Log += _hooks.HandleLog;


            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
