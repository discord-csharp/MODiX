using Discord;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Services.Messages.Discord;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using Modix.Services.Quote;
using System.Text;

namespace Modix.Services.Starboard
{
    public class StarboardHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
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

        public Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
            => HandleReaction(notification.Message, notification.Reaction);

        public Task Handle(ReactionRemoved notification, CancellationToken cancellationToken)
            => HandleReaction(notification.Message, notification.Reaction);

        private async Task HandleReaction(Cacheable<IUserMessage, ulong> cachedMessage, IReaction reaction)
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

            if (await _service.ExistsOnStarboard(message))
            {
                if (!_service.IsAboveReactionThreshold(message, emote))
                {
                    await _service.RemoveFromStarboard(channel.Guild, message);
                }
                else
                {
                    await _service.ModifyEntry(channel.Guild, message, FormatContent(message, emote));
                }
            }
            else if (_service.IsAboveReactionThreshold(message, emote))
            {
                var embed = GetStarEmbed(message);
                await _service.AddToStarboard(channel.Guild, message, FormatContent(message, emote), embed);
            }
        }

        private string FormatContent(IUserMessage message, IEmote emote)
        {
            var reactionCount = _service.GetReactionCount(message, emote);
            return $"**{reactionCount}** {_service.GetStarEmote(reactionCount)}";
        }

        private Embed GetStarEmbed(IUserMessage message)
        {
            var author = message.Author as IGuildUser;
            var embed = _quoteService.BuildQuoteEmbed(message, author)
                .WithTimestamp(message.Timestamp)
                .WithColor(new Color(255, 234, 174))
                .WithAuthor(
                    author.Nickname ?? author.Username,
                    author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl());

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
