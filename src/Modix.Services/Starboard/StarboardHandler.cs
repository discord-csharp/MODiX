using System.Threading;
using System.Threading.Tasks;
using Discord;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Quote;
using Modix.Services.Utilities;

namespace Modix.Services.Starboard
{
    public class StarboardHandler :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<ReactionRemovedNotification>
    {
        private readonly IStarboardService _starboardService;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IQuoteService _quoteService;

        public StarboardHandler(
            IStarboardService starboardService,
            IDesignatedChannelService designatedChannelService,
            IQuoteService quoteService)
        {
            _starboardService = starboardService;
            _designatedChannelService = designatedChannelService;
            _quoteService = quoteService;
        }

        public Task HandleNotificationAsync(ReactionAddedNotification notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        public Task HandleNotificationAsync(ReactionRemovedNotification notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cachedMessage, IReaction reaction)
        {
            var emote = reaction.Emote;
            if (!_starboardService.IsStarEmote(emote))
            {
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync();
            if (!(message.Channel is IGuildChannel channel))
            {
                return;
            }

            var isIgnoredFromStarboard = await _designatedChannelService
                .ChannelHasDesignationAsync(channel.Guild.Id, channel.Id, DesignatedChannelType.IgnoredFromStarboard, default);

            var starboardExists = await _designatedChannelService
                .AnyDesignatedChannelAsync(channel.GuildId, DesignatedChannelType.Starboard);

            if (isIgnoredFromStarboard || !starboardExists)
            {
                return;
            }

            var reactionCount = await _starboardService.GetReactionCount(message, emote);

            if (await _starboardService.ExistsOnStarboard(message))
            {
                if (_starboardService.IsAboveReactionThreshold(reactionCount))
                {
                    await _starboardService.ModifyEntry(channel.Guild, message, FormatContent(reactionCount), GetEmbedColor(reactionCount));
                }
                else
                {
                    await _starboardService.RemoveFromStarboard(channel.Guild, message);
                }
            }
            else if (_starboardService.IsAboveReactionThreshold(reactionCount))
            {
                var embed = GetStarEmbed(message, GetEmbedColor(reactionCount));

                if (embed is { })
                {
                    await _starboardService.AddToStarboard(channel.Guild, message, FormatContent(reactionCount), embed);
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
            return $"**{reactionCount}** {_starboardService.GetStarEmote(reactionCount)}";
        }

        private Embed GetStarEmbed(IUserMessage message, Color color)
        {
            var author = message.Author as IGuildUser;
            var embed = _quoteService.BuildQuoteEmbed(message, author);

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
