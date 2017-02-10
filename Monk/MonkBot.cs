using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monk
{
    class MonkBot
    {
        private CommandService commands = new CommandService();
        private DiscordSocketClient client = new DiscordSocketClient();
        private DependencyMap map = new DependencyMap();

        public async Task Run()
        {
            string token = "Mjc5Mjk1MzAyMzAzNzQ0MDAz.C3-tzQ.YsnAW2AASdjDxxCWlelWMtCHhw8";

            await InstallCommands();
            await Install(); // Setting up DependencyMap

            await client.LoginAsync(TokenType.Bot, token);
            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Install()
        {
            map.Add(client);
            map.Add(commands);
        }
    }
}
