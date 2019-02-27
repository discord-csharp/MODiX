using Discord;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Services.Messages.Discord;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using Discord.WebSocket;
using Modix.Services.Quote;

namespace Modix.Services.Starboard
{
    public class StarboardHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        private readonly StarboardService _service;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IQuoteService _quoteService;

        public StarboardHandler(
            StarboardService service,
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
            if (!(message.Channel is IGuildChannel channel)
                || await _designatedChannelService.ChannelHasDesignationAsync(
                    channel.Guild,
                    channel,
                    DesignatedChannelType.Unmoderated))
            {
                return;
            }

            var starboardMessage = await _service.GetFromStarboard(channel, message);
            if (starboardMessage != default)
            {
                if (!_service.IsAboveReactionThreshold(message, emote))
                {
                    await _service.RemoveFromStarboard(channel, message);
                }
                else
                {
                    await starboardMessage.ModifyAsync(messageProps
                        => messageProps.Content = FormatContent(message, emote));
                }
            }
            else
            {
                if (_service.IsAboveReactionThreshold(message, emote))
                {
                    var embed = GetStarEmbed(message);
                    await _designatedChannelService.SendToDesignatedChannelsAsync(
                        channel.Guild,
                        DesignatedChannelType.Starboard,
                        FormatContent(message, emote),
                        embed);
                }
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
            var builder = _quoteService.BuildQuoteEmbed(message, author)
                .WithTimestamp(message.Timestamp)
                .WithColor(new Color(255, 234, 174))
                .WithAuthor(
                    author.Nickname ?? author.Username,
                    author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl());

            builder.Fields.RemoveAt(builder.Fields.Count-1); //Remove the "Quoted by" field
            builder.Footer = null;

            if (message.Embeds.Count == 0)
            {
                builder.Description = null;
                builder.AddField("Message", $"{message.Content}");
            }
            builder.AddField("\u200B", $"_Posted in [**#{message.Channel.Name}**]({message.GetJumpUrl()})_");
            //------------------^ zero-width character
            return builder.Build();
        }
    }
}
