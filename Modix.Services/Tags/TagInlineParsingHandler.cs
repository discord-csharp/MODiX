using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Tags
{
    public class TagInlineParsingHandler : INotificationHandler<MessageReceivedNotification>
    {
        private static readonly Regex _inlineTagRegex = new(@"\$(\S+)\b");

        public DiscordSocketClient DiscordClient { get; }
        public IAuthorizationService AuthorizationService { get; }
        public ITagService TagService { get; }

        public TagInlineParsingHandler(DiscordSocketClient discordClient, IAuthorizationService authorizationService, ITagService tagService)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            TagService = tagService;
        }

        public async Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;

            if (message is not SocketUserMessage userMessage || userMessage.Author.IsBot) { return; }
            if (userMessage.Author is not SocketGuildUser guildUser) { return; }

            //TODO: Refactor when we have a configurable prefix
            if (message.Content.StartsWith('!')) { return; }

            //Remove code blocks from the message we are processing
            var content = Regex.Replace(message.Content, @"(`{1,3}).*?(.\1)", string.Empty, RegexOptions.Singleline);
            //Remove quotes from the message we are processing
            content = Regex.Replace(content, "^>.*$", string.Empty, RegexOptions.Multiline);

            if (string.IsNullOrWhiteSpace(content)) { return; }

            var match = _inlineTagRegex.Match(content);
            if (!match.Success) { return; }

            var tagName = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(tagName)) { return; }

            if (await AuthorizationService.HasClaimsAsync(guildUser.Id, guildUser.Guild.Id, guildUser.Roles.Select(x => x.Id).ToList(), AuthorizationClaim.UseTag) == false) { return; }
            if (await TagService.TagExistsAsync(guildUser.Guild.Id, tagName) == false) { return; }

            try
            {
                await AuthorizationService.OnAuthenticatedAsync(guildUser.Id, guildUser.Guild.Id, guildUser.Roles.Select(x => x.Id).ToList());
                await TagService.UseTagAsync(guildUser.Guild.Id, userMessage.Channel.Id, tagName, message);
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
