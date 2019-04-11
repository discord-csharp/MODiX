using Discord;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IStarboardService _service;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IQuoteService _quoteService;

        public StarboardHandler(
            IStarboardService service,
            IDesignatedChannelService designatedChannelService,
            IQuoteService quoteService)
        {
            _service = service;
            _designatedChannelService = designatedChannelService;
            _quoteService = quoteService;
        }

        public Task HandleNotificationAsync(ReactionAddedNotification notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        public Task HandleNotificationAsync(ReactionRemovedNotification notification, CancellationToken cancellationToken)
            => HandleReactionAsync(notification.Message, notification.Reaction);

        private async Task HandleReactionAsync(ICacheable<IUserMessage, ulong> cachedMessage, IReaction reaction)
        {
            var emote = reaction.Emote;
            if (!_service.IsStarEmote(emote))
            {
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync();
            if (!(message.Channel is IGuildChannel channel))
            {
                return;
            }

            bool isUnmoderated = await _designatedChannelService
                .ChannelHasDesignationAsync(channel.Guild, channel, DesignatedChannelType.Unmoderated);

            bool starboardExists = await _designatedChannelService
                .AnyDesignatedChannelAsync(channel.GuildId, DesignatedChannelType.Starboard);
            if (isUnmoderated || !starboardExists)
            {
                return;
            }

            int reactionCount = await _service.GetReactionCount(message, emote);
            if (await _service.ExistsOnStarboard(message))
            {
                if (_service.IsAboveReactionThreshold(reactionCount))
                {
                    await _service.ModifyEntry(channel.Guild, message, FormatContent(reactionCount), GetEmbedColor(reactionCount));
                }
                else
                {
                    await _service.RemoveFromStarboard(channel.Guild, message);
                }
            }
            else if (_service.IsAboveReactionThreshold(reactionCount))
            {
                var embed = GetStarEmbed(message, GetEmbedColor(reactionCount));
                await _service.AddToStarboard(channel.Guild, message, FormatContent(reactionCount), embed);
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
            return $"**{reactionCount}** {_service.GetStarEmote(reactionCount)}";
        }

        private Embed GetStarEmbed(IUserMessage message, Color color)
        {
            var author = message.Author as IGuildUser;
            var embed = _quoteService.BuildQuoteEmbed(message, author)
                .WithTimestamp(message.Timestamp)
                .WithColor(color)
                .WithUserAsAuthor(author);

            embed.Description = new StringBuilder()
                .AppendLine($"_Posted in **[#{message.Channel.Name}]({message.GetJumpUrl()})**_")
                .AppendLine()
                .AppendLine("**Message**")
                .AppendLine(embed.Description)
                .ToString();

            embed.Fields.RemoveAt(embed.Fields.Count-1); //Remove the "Quoted by" field
            embed.Footer = null;
            return embed.Build();
        }
    }
}
