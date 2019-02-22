using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Services.Messages.Discord;
using Modix.Services.Quote;
using Modix.Services.Core;
using System;
using Modix.Data.Models.Core;

namespace Modix.Services.Starboard
{

    public class StarboardHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        private StarboardService _service;
        private IQuoteService _quoteService;
        private IDesignatedChannelService _designatedChannelService;
        public StarboardHandler(StarboardService service, IQuoteService quoteService, IDesignatedChannelService designatedChannelService)
        {
            _service = service;
            _quoteService = quoteService;
            _designatedChannelService = designatedChannelService;
        }
        
        public async Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
        {
            var reaction = notification.Reaction;
            if(!await _service.IsStarReaction(reaction))
            {
                return;
            }

            var message = await notification.Message.GetOrDownloadAsync();
            if (!(message.Channel is IGuildChannel channel))
            {
                return;
            }

            if (await _service.IsAboveReactionThreshold(message, notification))
            {

                //Get existing message from starboardservice?
                //if != null -> edit with returned message (selfmessage)?
                var embed = _quoteService
                    .BuildQuoteEmbed(message, message.Author)
                    .AddField("Posted by", message.Author.Mention, true)
                    .WithColor(new Color(255, 234, 174))
                    .Build();

                var quoteUrl = await _service.BuildQuoteUrl(channel, message);
                await _designatedChannelService.SendToDesignatedChannelsAsync(
                    channel.Guild,
                    DesignatedChannelType.Starboard,
                    $"**{await _service.GetReactionCount(message, notification)}** {await GetStarEmote(message, notification)} {quoteUrl}",
                    embed);
            }
        }

        public async Task<string> GetStarEmote(IUserMessage message, ReactionAdded notification)
        {
            var reactionCount = await _service.GetReactionCount(message, notification);
            return reactionCount >= 5 ? _service.GreaterEmote : _service.ReactionEmote;
        }

        public Task Handle(ReactionRemoved notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();  }
            //=> ModifyRatings(notification.Message, notification.Reaction);

    }
}
