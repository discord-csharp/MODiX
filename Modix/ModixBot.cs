using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix
{
    public sealed class ModixBot
    {
        private readonly CommandService _commands = new CommandService();
        private readonly DiscordSocketClient _client = new DiscordSocketClient();
        private readonly DependencyMap _map = new DependencyMap();

        public async Task Run()
        {
            var token = Environment.GetEnvironmentVariable("MODIX_BOT_KEY");

            await Install(); // Setting up DependencyMap

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
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
        }

        public async Task Install()
        {
            _map.Add(_client);
            _map.Add(_commands);

            _client.MessageReceived += HandleCommand;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
