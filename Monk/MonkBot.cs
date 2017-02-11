using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Monk.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monk
{
    public sealed class MonkBot
    {
        private CommandService commands = new CommandService();
        private DiscordSocketClient client = new DiscordSocketClient();
        private DependencyMap map = new DependencyMap();

        public async Task Run()
        {
            string token = "Mjc5Nzk2MzAwMjEwMzcyNjEy.C4AENQ.CcUpTsuOjPckDy_r1cCeNsxxkT0";

            await Install(); // Setting up DependencyMap

            await client.LoginAsync(TokenType.Bot, token);
            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos) || IsCustomCommand(message)))
                return;

            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public bool IsCustomCommand(SocketUserMessage message)
        {
            var customs = new List<string>();
            var isCustom = false;

            customs.Add("thanks");
            
            customs.ForEach(x =>
            {
                if (message.Content.ToLower().Contains(x.ToLower()))
                    isCustom = true;
            });

            return isCustom;
        }

        public async Task Install()
        {
            map.Add(client);
            map.Add(commands);

            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
