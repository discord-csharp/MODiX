using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;

namespace Modix.Services.Starboard
{
    public class StarboardService
    {
        private const string _baseQuoteUrl = "https://discordapp.com/channels";
        public string ReactionEmote { get; } = "⭐";
        public string GreaterEmote { get; } = "🌟";

        private IDiscordClient _discordClient;

        public StarboardService(IDiscordClient discordClient, IDesignatedChannelService designatedChannelService)
        {
            _discordClient = discordClient;
        }

        //TODO: Function<bool>: => Starred message exists in designated starboard channel

        //TODO: Function: => Delete starred message in designated starboard channel

        //TODO: Function: => Update starred message in designated starboard channel


        public bool IsStarReaction(SocketReaction reaction)
            => reaction.Emote.Name == ReactionEmote;

        public int GetReactionCount(IUserMessage message, ReactionAdded reaction)
            => message.Reactions[reaction.Reaction.Emote].ReactionCount;

        public bool IsAboveReactionThreshold(IUserMessage message, ReactionAdded reaction)
            => GetReactionCount(message, reaction) >= 2;

        public string BuildQuoteUrl(IGuildChannel channel, IMessage message)
            => string.Join("/", _baseQuoteUrl, channel.Guild.Id, channel.Id, message.Author.Id);
    }
}
