using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Tags
{
    public class TagInlineParserBehavior : BehaviorBase
    {
        private static readonly Regex _inlineTagRegex = new Regex(@"\$(\S+)\s?");

        public DiscordSocketClient DiscordClient { get; }

        public TagInlineParserBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        internal protected override Task OnStartingAsync()
        {
            DiscordClient.MessageReceived += Handle;
            return Task.CompletedTask;
        }

        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.MessageReceived -= Handle;
            return Task.CompletedTask;
        }

        private async Task Handle(IMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) { return; }
            if (!(userMessage.Author is SocketGuildUser guildUser)) { return; }

            var match = _inlineTagRegex.Match(message.Content);
            if (!match.Success) { return; }

            var tagName = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(tagName)) { return; }

            await SelfExecuteRequest<IAuthorizationService, ITagService>(async (authService, tagService) =>
            {
                if (await authService.HasClaimsAsync(guildUser, AuthorizationClaim.UseTag) == false) { return; }
                if (await tagService.TagExistsAsync(guildUser.Guild.Id, tagName) == false) { return; }

                try
                {
                    await tagService.UseTagAsync(guildUser.Guild.Id, userMessage.Channel.Id, tagName);
                }
                catch (InvalidOperationException ex)
                {
                    var embed = new EmbedBuilder()
                        .WithTitle("Error invoking inline tag")
                        .WithColor(Color.Red)
                        .WithDescription(ex.Message);

                    await userMessage.Channel.SendMessageAsync(embed: embed.Build());
                }
            });
        }
    }
}
