using Discord;
using Discord.WebSocket;
using Humanizer;
using Modix.Data.Models.Core;
using Modix.Services.AutoCodePaste;
using Modix.Services.Core;
using Modix.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.PopularityContest
{
    public interface IPopularityContestService
    {
        /// <summary>
        /// Begins the data collection process for a popularity contest (with an optional role filter), and logs the results
        /// </summary>
        /// <param name="countedEmote">The reaction to count for the contest</param>
        /// <param name="collectionChannel">The channel to count reactions in</param>
        /// <param name="logChannel">The channel to log the results of the count</param>
        /// <param name="roleFilter">An optional set of roles, that a message's author must have one of to be counted</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task CollectDataAsync(IEmote countedEmote, ISocketMessageChannel collectionChannel, ISocketMessageChannel logChannel, IEnumerable<IRole> roleFilter);
    }

    public class PopularityContestService : IPopularityContestService
    {
        private readonly DiscordSocketClient _client;
        private readonly CodePasteService _pasteService;
        private readonly IAuthorizationService _authorizationService;

        public PopularityContestService(DiscordSocketClient client, CodePasteService pasteService, IAuthorizationService authorizationService)
        {
            _client = client;
            _pasteService = pasteService;
            _authorizationService = authorizationService;
        }

        private async Task<(IEnumerable<IUserMessage> messages, IUserMessage logMessage)> GetMessagesInChannelAsync(ISocketMessageChannel collectionChannel, ISocketMessageChannel logChannel)
        {
            if (!(collectionChannel is IGuildChannel guildChannel))
            {
                throw new ArgumentException("Collection channel must be a guild channel");
            }

            //We send a message at the start of the process and reuse it to update progress, and display the final results
            var logMessage = await logChannel.SendMessageAsync("", embed: GetProgressEmbed(0, null, guildChannel));

            IMessage offset = null;
            var ret = new List<IMessage>();

            IEnumerable<IMessage> lastBatch;
            do
            {
                if (offset != null)
                {
                    lastBatch = await collectionChannel.GetMessagesAsync(offset, Direction.Before).FlattenAsync();
                }
                else
                {
                    lastBatch = await collectionChannel.GetMessagesAsync().FlattenAsync();
                }

                ret.AddRange(lastBatch);
                offset = ret.Last();

                if (ret.Count % 500 == 0)
                {
                    await logMessage.ModifyAsync(prop => prop.Embed = GetProgressEmbed(ret.Count, offset, guildChannel));
                }
            }
            while (lastBatch.Count() > 0);

            await logMessage.ModifyAsync(prop => prop.Embed = GetProgressEmbed(ret.Count, offset, guildChannel, true));

            return (ret.OfType<IUserMessage>(), logMessage);
        }

        private bool UserIsOldEnough(IGuildUser user)
        {
            if (user.JoinedAt == null) { return false; }
            return user.JoinedAt <= DateTimeOffset.FromUnixTimeSeconds(1538858460);
        }

        /// <inheritdoc />
        public async Task CollectDataAsync(IEmote countedEmote, ISocketMessageChannel collectionChannel, ISocketMessageChannel logChannel, IEnumerable<IRole> roleFilter)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.PopularityContestCount);

            (var messages, var logMessage) = await GetMessagesInChannelAsync(collectionChannel, logChannel);
            var roleIds = roleFilter.Select(d => d.Id).ToArray();

            //Get the last message from each user that has more than 0 reactions, at least one reaction of the kind
            //we're looking for, where the user joined the server before the contest message was sent (10/06/2018 8:41pm UTC),
            //and, if the role filter is specified, has one of those roles
            var lastMessages = messages
                .Where(d => d.Reactions.Count > 0)
                .Where(d =>
                {
                    if (!(d.Author is IGuildUser msgAuthor)) { return false; }
                    if (!UserIsOldEnough(msgAuthor)) { return false; }
                    if (roleFilter == null || roleFilter.Count() == 0) { return true; }

                    return msgAuthor.RoleIds.Intersect(roleIds).Any();
                })
                .OrderByDescending(d => d.CreatedAt)
                .GroupBy(d => d.Author)
                .Select(d => d.First());

            //Take the last message from each user, ordered by reaction count, and take up to the top 3 entries
            var mostReactedMessages = lastMessages
                .Where(message => message.Reactions.ContainsKey(countedEmote))
                .Select(message => (message, message.Reactions[countedEmote].ReactionCount))
                .OrderByDescending(d => d.ReactionCount)
                .Take(3);

            if (mostReactedMessages == null || mostReactedMessages.Count() == 0)
            {
                await logChannel.SendMessageAsync
                (
                    "Uh oh, we didn't find any messages that matched your criteria.", 
                    embed: new EmbedBuilder()
                        .AddField("Channel", MentionUtils.MentionChannel(collectionChannel.Id), true)
                        .AddField("Emoji", countedEmote, true)
                        .AddField("Roles", roleFilter.Any() ? roleFilter.Select(d => d.Name).Humanize() : "No Filter", true)
                        .Build()
                );

                return;
            }

            var paste = await UploadLogAsync(logMessage, countedEmote, lastMessages);
            
            var isMultiple = mostReactedMessages.Count() > 1;

            var embed = new EmbedBuilder()
                .WithTitle($"Counting complete!")
                .WithDescription($"Out of **{lastMessages.Count()}** entries, the top {mostReactedMessages.Count()} messages with the most {countedEmote} reactions " +
                    (isMultiple ? "are..." : "is..."))
                .WithColor(new Color(0, 200, 0))
                .WithFooter($"See all entries here: {paste}");

            var position = 1;
            foreach ((var message, var reactionCount) in mostReactedMessages)
            {
                var author = message.Author;
                var emoji = "";

                switch (position)
                {
                    case 1:
                        emoji = "🥇";
                        break;
                    case 2:
                        emoji = "🥈";
                        break;
                    case 3:
                        emoji = "🥉";
                        break;
                    default:
                        emoji = "🏅";
                        break;
                }

                embed.AddField($"{emoji} With **{reactionCount}** votes",
                    $"[{author.Username}#{author.Discriminator}! (`{author.Id}`)]({message.GetMessageLink()})");

                position++;
            }

            var mostReactionsOverall = lastMessages
                .Select(message => new { Message = message, OverallCount = message.Reactions.Values.Sum(r => r.ReactionCount) })
                .OrderByDescending(d => d.OverallCount)
                .First();

            var mostAuthor = mostReactionsOverall.Message.Author;
            embed.AddField($"Also, the message with the most reactions overall, with a total of **{mostReactionsOverall.OverallCount}**, is...",
                $"[{mostAuthor.Username}#{mostAuthor.Discriminator}! (`{mostAuthor.Id}`)]({mostReactionsOverall.Message.GetMessageLink()})");

            await logMessage.ModifyAsync(prop => prop.Embed = embed.Build());
        }

        private async Task<string> UploadLogAsync(IMessage logMessage, IEmote countedEmote, IEnumerable<IUserMessage> messages)
        {
            messages = messages.Where(message => message.Reactions.ContainsKey(countedEmote));
            var allEntries = string.Join('\n', messages.Select(d => $"{d.Reactions[countedEmote].ReactionCount} reactions: {d.GetMessageLink()}"));

            return await _pasteService.UploadCodeAsync($"All entries for contest held on {DateTimeOffset.UtcNow}\nResults here: {logMessage.GetMessageLink()}\n\n{allEntries}");
        }

        private Embed GetProgressEmbed(int progress, IMessage lastMessage, IGuildChannel collectionChannel, bool done = false)
        {
            var messageLink = "";
            if (lastMessage != null)
            {
                messageLink = lastMessage.GetMessageLink();
            }

            var embed = new EmbedBuilder()
                .WithTitle(done ? $"Done collecting from #{collectionChannel.Name}!" : $"Collecting messages from #{collectionChannel.Name}...")
                .AddField("Total Message Count", $"{progress}")
                .AddField("Last Message", $"[{lastMessage?.CreatedAt.ToString() ?? "None"}]({messageLink})")
                .WithColor(done ? new Color(0, 200, 0) : new Color(218, 165, 32))
                .WithTimestamp(DateTimeOffset.UtcNow);

            if (done)
            {
                embed = embed.WithDescription("Counting the votes...");
            }

            return embed.Build();
        }
    }
}
