using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Data.Models.Core;

namespace Modix.Services.Starboard
{
    public class StarboardService
    {
        private const string _baseContextUrl = "https://discordapp.com/channels";

        //TODO: Dictionary<Emoji, int>
        public string GoodEmote { get; } = "⭐";
        public string GreatEmote { get; } = "🌟";
        public string GreaterEmote { get; } = "💫";
        public string GreatestEmote { get; } = "✨";

        private IDesignatedChannelService _designatedChannelService;

        public StarboardService(IDesignatedChannelService designatedChannelService)
        {
            _designatedChannelService = designatedChannelService;
        }

        public async Task<IUserMessage> GetFromStarboard(IGuildChannel channel, IMessage message)
        {
            var starboardChannels = await _designatedChannelService
                .GetDesignatedChannelsAsync(channel.Guild, DesignatedChannelType.Starboard);

            var starMessages = await starboardChannels
                .First()
                .GetMessagesAsync()
                .FlattenAsync();

            return starMessages
                .Cast<IUserMessage>()
                .FirstOrDefault(x => x.Content.EndsWith(message.Id.ToString()));
        }

        public async Task RemoveFromStarboard(IGuildChannel channel, IMessage message)
        {
            var messageToRemove = await GetFromStarboard(channel, message);
            await messageToRemove.DeleteAsync();
        }

        public bool IsStarReaction(SocketReaction reaction)
            => reaction.Emote.Name == GoodEmote;

        public int GetReactionCount(IUserMessage message, IReaction reaction)
        {
            if (!message.Reactions.TryGetValue(reaction.Emote, out var metadata))
                return 0;
            return message.Reactions[reaction.Emote].ReactionCount;
        }

        public bool IsAboveReactionThreshold(IUserMessage message, IReaction reaction)
            => GetReactionCount(message, reaction) >= 2;

        public string BuildContextUrl(IGuildChannel channel, IMessage message)
            => string.Join("/", _baseContextUrl, channel.Guild.Id, channel.Id, message.Author.Id);
    }
}
