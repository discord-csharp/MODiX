using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;

namespace Modix.Services.Tags
{
    public class TagInlineParsingHandler : INotificationHandler<ChatMessageReceived>
    {
        private static readonly Regex _inlineTagRegex = new Regex(@"\$(\S+)\s?");

        public DiscordSocketClient DiscordClient { get; }
        public IAuthorizationService AuthorizationService { get; }
        public ITagService TagService { get; }

        public TagInlineParsingHandler(DiscordSocketClient discordClient, IAuthorizationService authorizationService, ITagService tagService)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            TagService = tagService;
        }

        public async Task Handle(ChatMessageReceived notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;

            if (!(message is SocketUserMessage userMessage) || userMessage.Author.IsBot) { return; }
            if (!(userMessage.Author is SocketGuildUser guildUser)) { return; }

            //TODO: Refactor when we have a configurable prefix
            if (message.Content.StartsWith('!')) { return; }

            var match = _inlineTagRegex.Match(message.Content);
            if (!match.Success) { return; }

            var tagName = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(tagName)) { return; }

            if (await AuthorizationService.HasClaimsAsync(guildUser, AuthorizationClaim.UseTag) == false) { return; }
            if (await TagService.TagExistsAsync(guildUser.Guild.Id, tagName) == false) { return; }

            try
            {
                await TagService.UseTagAsync(guildUser.Guild.Id, userMessage.Channel.Id, tagName);
            }
            catch (InvalidOperationException ex)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Error invoking inline tag")
                    .WithColor(Color.Red)
                    .WithDescription(ex.Message);

                await userMessage.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}
