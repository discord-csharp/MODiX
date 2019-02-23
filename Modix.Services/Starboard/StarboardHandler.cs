using Discord;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Services.Messages.Discord;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using Discord.WebSocket;

namespace Modix.Services.Starboard
{
    public class StarboardHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        private readonly StarboardService _service;
        private readonly IDesignatedChannelService _designatedChannelService;
        private const string _contentString = "**{0}** {1} ID: {2}";

        public StarboardHandler(StarboardService service, IDesignatedChannelService designatedChannelService)
        {
            _service = service;
            _designatedChannelService = designatedChannelService;
        }

        public Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
            => HandleReaction(notification.Message, notification.Reaction);

        public Task Handle(ReactionRemoved notification, CancellationToken cancellationToken)
            => HandleReaction(notification.Message, notification.Reaction);

        private async Task HandleReaction(Cacheable<IUserMessage, ulong> cachedMessage, SocketReaction reaction)
        {
            if (!_service.IsStarReaction(reaction))
            {
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync();
            if (!(message.Channel is IGuildChannel channel))
            {
                return;
            }

            var starboardMessage = await _service.GetFromStarboard(channel, message);
            if (starboardMessage != default)
            {
                var embed = GetStarEmbed(channel, message);
                var quoteUrl = _service.BuildContextUrl(channel, message);

                if (!_service.IsAboveReactionThreshold(message, reaction))
                {
                    await _service.RemoveFromStarboard(channel, message);
                }
                else
                {
                    await starboardMessage.ModifyAsync((messageProperties) =>
                    {
                        messageProperties.Embed = embed;
                        messageProperties.Content = FormatContent(message, reaction);
                    });
                }
            }
            else
            {
                if (_service.IsAboveReactionThreshold(message, reaction))
                {
                    var embed = GetStarEmbed(channel, message);
                    var quoteUrl = _service.BuildContextUrl(channel, message);

                    await _designatedChannelService.SendToDesignatedChannelsAsync(
                        channel.Guild,
                        DesignatedChannelType.Starboard,
                        FormatContent(message, reaction),
                        embed);
                }
            }
        }

        public string FormatContent(IUserMessage message, IReaction reaction)
        {
            var reactionCount = _service.GetReactionCount(message, reaction);
            return string.Format(_contentString, reactionCount, GetStarEmote(reactionCount), message.Id);
        }

        //TODO: Figure out where to put clickable link to original message
        // Preferably in markdown format --> [Show More](link)
        public Embed GetStarEmbed(IGuildChannel channel, IUserMessage message)
        {
            var author = message.Author as IGuildUser;
            var builder = new EmbedBuilder()
                .WithTimestamp(message.Timestamp)
                .WithColor(new Color(255, 234, 174))
                .WithAuthor(author.Nickname ?? author.Username, author.GetAvatarUrl())
                .WithDescription(message.Content)
                .WithFooter($"Posted in #{message.Channel.Name}");
            //embed.Author.Url = _service.BuildContextUrl(channel, message);

            return builder.Build();
        }

        public string GetStarEmote(int reactionCount)
        {
            if (reactionCount >= 20) return _service.GreatestEmote;
            else if (reactionCount >= 10) return _service.GreaterEmote;
            else if (reactionCount >= 5) return _service.GreatEmote;
            else return _service.GoodEmote;
        }
    }
}
