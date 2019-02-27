using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using System.Collections.Generic;

namespace Modix.Services.Starboard
{
    /// <summary>
    /// Defines a service that assists with handling a starboard channel.
    /// </summary>
    public interface IStarboardService
    {
        /// <summary>
        /// Fetches a message from the specified guilds starboard.
        /// </summary>
        /// <param name="channel">Which guild's starboard to fetch from</param>
        /// <param name="message">The message to fetch</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the an <see cref="IMessage"/> if found, otherwise returns a default value.
        /// </returns>
        Task<IUserMessage> GetFromStarboard(IGuildChannel channel, IMessage message);

        /// <summary>
        /// Removes a message from the specified guilds starboard.
        /// </summary>
        /// <param name="channel">Which guilds starboard to operate on</param>
        /// <param name="message">The message to delete</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveFromStarboard(IGuildChannel channel, IMessage message);

        /// <summary>
        /// Checks whether an <see cref="IEmote"/> is a star or not.
        /// </summary>
        /// <param name="emote">The emote to evaluate</param>
        /// <returns>A flag indicating whether <paramref name="emote"/> is a star.</returns>
        bool IsStarEmote(IEmote emote);

        /// <summary>
        /// Checks if an <see cref="IEmote"/>-reaction on a given <see cref="IUserMessage"/> is above the threshold.
        /// </summary>
        /// <param name="message">The message to examine</param>
        /// <param name="emote">The emote to evaluate</param>
        /// <returns>A flag indicating whether reactions of type <paramref name="emote"/> is above the threshold on <paramref name="message"/>.</returns>
        bool IsAboveReactionThreshold(IUserMessage message, IEmote emote);

        /// <summary>
        /// Gets the current amount of reactions of the given <paramref name="emote"/> on the <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message to examine</param>
        /// <param name="emote">The emote to evaluate</param>
        int GetReactionCount(IUserMessage message, IEmote emote);

        /// <summary>
        /// Gets the appropriate star-emote for the given <paramref name="reactionCount"/>.
        /// </summary>
        /// <param name="reactionCount"></param>
        /// <returns>A star-emote in string format</returns>
        string GetStarEmote(int reactionCount);
    }

    /// <inheritdoc />
    public class StarboardService : IStarboardService
    {
        private readonly IDesignatedChannelService _designatedChannelService;
        private static readonly IReadOnlyDictionary<int, string> _emojis = new Dictionary<int, string>
        {
            { 20, "✨"},
            { 10, "💫" },
            { 5, "🌟" },
            { 0, "⭐" }
        }.OrderByDescending(k => k.Key).ToDictionary(k => k.Key, k => k.Value);

        public StarboardService(IDesignatedChannelService designatedChannelService)
        {
            _designatedChannelService = designatedChannelService;
        }

        /// <inheritdoc />
        public async Task<IUserMessage> GetFromStarboard(IGuildChannel channel, IMessage message)
        {
            var starboardChannels = await _designatedChannelService
                .GetDesignatedChannelsAsync(channel.Guild, DesignatedChannelType.Starboard);

            var starMessages = await starboardChannels
                .First()
                .GetMessagesAsync()
                .FlattenAsync();

            //We need to store entries in the db to avoid this horrible mess
            return starMessages
                .Cast<IUserMessage>()
                .FirstOrDefault(x => x.Embeds
                                        .Select(y => y.Fields.Last())
                                        .Any(y => y.Value.Contains(message.GetJumpUrl())));
        }

        /// <inheritdoc />
        public async Task RemoveFromStarboard(IGuildChannel channel, IMessage message)
        {
            var messageToRemove = await GetFromStarboard(channel, message);
            await messageToRemove.DeleteAsync();
        }

        /// <inheritdoc />
        public bool IsStarEmote(IEmote emote)
            => emote.Name == _emojis.Values.Last();

        /// <inheritdoc />
        public int GetReactionCount(IUserMessage message, IEmote emote)
        {
            if (!message.Reactions.TryGetValue(emote, out var metadata))
                return 0;
            return metadata.ReactionCount;
        }

        /// <inheritdoc />
        public bool IsAboveReactionThreshold(IUserMessage message, IEmote emote)
            => GetReactionCount(message, emote) >= 2;

        public string GetStarEmote(int reactionCount)
        {
            var emoteIndex = _emojis.Keys.First((val) => reactionCount >= val);
            return _emojis[emoteIndex];
        }
    }
}
