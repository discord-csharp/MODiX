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


        public Task<bool> IsStarReaction(SocketReaction reaction)
            => Task.FromResult(reaction.Emote.Name == ReactionEmote);

        public Task<int> GetReactionCount(IUserMessage message, ReactionAdded reaction)
            => Task.FromResult(message.Reactions[reaction.Reaction.Emote].ReactionCount);

        public async Task<bool> IsAboveReactionThreshold(IUserMessage message, ReactionAdded reaction)
        {
            return await GetReactionCount(message, reaction) >= 2;
        }

        public Task<string> BuildQuoteUrl(IGuildChannel channel, IMessage message)
            => Task.FromResult(string.Join("/", _baseQuoteUrl, channel.Guild.Id, channel.Id, message.Author.Id));
    }
}
