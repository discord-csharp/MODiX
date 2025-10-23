using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Bot.Responders.MessageQuotes;
using Modix.Data.Models.Core;
using Modix.Services;
using Modix.Services.Starboard;
using Modix.Services.Utilities;

namespace Modix.Bot.Responders
{
    public class StarboardReactionResponder(IStarboardService starboardService, DesignatedChannelService designatedChannelService)
        : INotificationHandler<ReactionAddedNotificationV3>, INotificationHandler<ReactionRemovedNotificationV3>
    {
        public Task Handle(ReactionAddedNotificationV3 notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        public Task Handle(ReactionRemovedNotificationV3 notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        private async ValueTask<bool> IsChannelIgnoredFromStarboard(IGuildChannel channel)
        {
            if (channel.ChannelType is ChannelType.PrivateThread)
            {
                return true;
            }

            var hasDirectDesignation = await designatedChannelService
                .ChannelHasDesignation(channel.Guild.Id, channel.Id, DesignatedChannelType.IgnoredFromStarboard, default);

            if (hasDirectDesignation)
            {
                return true;
            }

            if (channel is SocketThreadChannel { ParentChannel: not null } threadChannel)
            {
                var parentChannelId = threadChannel.ParentChannel.Id;

                var parentHasDesignation = await designatedChannelService
                    .ChannelHasDesignation(channel.Guild.Id, parentChannelId, DesignatedChannelType.IgnoredFromStarboard, default);

                if (parentHasDesignation)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cachedMessage, IReaction reaction)
        {
            var emote = reaction.Emote;
            if (!starboardService.IsStarEmote(emote))
            {
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync();
            if (!(message.Channel is IGuildChannel channel))
            {
                return;
            }

            if (await IsChannelIgnoredFromStarboard(channel))
            {
                return;
            }

            var starboardExists = await designatedChannelService
                .HasDesignatedChannelForType(channel.GuildId, DesignatedChannelType.Starboard);

            if (!starboardExists)
            {
                return;
            }

            var reactionCount = await starboardService.GetReactionCount(message, emote);

            if (await starboardService.ExistsOnStarboard(message))
            {
                if (starboardService.IsAboveReactionThreshold(reactionCount))
                {
                    await starboardService.ModifyEntry(channel.Guild, message, FormatContent(reactionCount), GetEmbedColor(reactionCount));
                }
                else
                {
                    await starboardService.RemoveFromStarboard(channel.Guild, message);
                }
            }
            else if (starboardService.IsAboveReactionThreshold(reactionCount))
            {
                var embed = GetStarEmbed(message, GetEmbedColor(reactionCount));

                if (embed is { })
                {
                    await starboardService.AddToStarboard(channel.Guild, message, FormatContent(reactionCount), embed);
                }
            }
        }

        private Color GetEmbedColor(int reactionCount)
        {
            var percentModifier = reactionCount / 15.0;
            if (percentModifier > 1.0)
                percentModifier = 1;

            int r, g, b;
            r = Color.Gold.R;
            g = (int)((Color.Gold.G * percentModifier) + (240 * (1 - percentModifier)));
            b = (int)((Color.Gold.B * percentModifier) + (220 * (1 - percentModifier)));

            return new Color(r, g, b);
        }

        private string FormatContent(int reactionCount)
        {
            return $"**{reactionCount}** {starboardService.GetStarEmote(reactionCount)}";
        }

        private Embed GetStarEmbed(IUserMessage message, Color color)
        {
            var author = message.Author as IGuildUser;
            var embed = MessageQuoteEmbedHelper.BuildQuoteEmbed(message, author);

            if (embed is null)
            {
                return null;
            }

            embed.WithTimestamp(message.Timestamp)
                .WithColor(color)
                .WithUserAsAuthor(author)
                .WithFooter(string.Empty)
                .AddField("Posted in", $"**{message.GetJumpUrlForEmbed()}**");

            embed.Fields.RemoveAll(x => x.Name == "Quoted by");

            return embed.Build();
        }
    }
}
