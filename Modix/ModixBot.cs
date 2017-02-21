using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NLog;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService();
        private DiscordSocketClient _client;
        private readonly DependencyMap _map = new DependencyMap();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task Run()
        {
            var token = Environment.GetEnvironmentVariable("MODIX_BOT_KEY");
            _client = new DiscordSocketClient(config: new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
            });

            await Install(); // Setting up DependencyMap

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.ConnectAsync();
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
            Logger.Info($"{stopwatch.ElapsedMilliseconds}ms: {message}");
        }

        public async Task Install()
        {
            _map.Add(_client);
            _map.Add(_commands);

            _client.MessageReceived += HandleCommand;
            _client.Log += (message) =>
            {
                Logger.Info(message.ToString());
                return Task.CompletedTask;
            };
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
