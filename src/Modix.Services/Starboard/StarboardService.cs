using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using System.Collections.Generic;
using Modix.Data.Repositories;

namespace Modix.Services.Starboard
{
    /// <summary>
    /// Defines a service that assists with handling a starboard channel.
    /// </summary>
    public interface IStarboardService
    {
        /// <summary>
        /// Checks whether a <see cref="IMessage"/> exists as an entry on the starboard.
        /// </summary>
        /// <param name="message">The message to look for</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed, containing a flag indicating whether the <paramref name="message"/>exists on the starboard or not.</returns>
        Task<bool> ExistsOnStarboard(IMessage message);

        /// <summary>
        /// Removes a message from the specified guilds starboard.
        /// </summary>
        /// <param name="guild">Which guild's starboard to operate on</param>
        /// <param name="message">The message to delete</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveFromStarboard(IGuild guild, IMessage message);

        /// <summary>
        /// Checks whether an <see cref="IEmote"/> is a star or not.
        /// </summary>
        /// <param name="emote">The emote to evaluate</param>
        /// <returns>A flag indicating whether <paramref name="emote"/> is a star.</returns>
        bool IsStarEmote(IEmote emote);

        /// <summary>
        /// Checks if a reaction count is above the threshold.
        /// </summary>
        /// <param name="reactionCount"> The reaction count to evaluate</param>
        /// <returns>A flag indicating whether the reaction count is above the threshold.</returns>
        bool IsAboveReactionThreshold(int reactionCount);

        /// <summary>
        /// Gets the current amount of reactions of the given <paramref name="emote"/> on the <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message to examine</param>
        /// <param name="emote">The emote to evaluate</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed, containing the amount of star-reactions on the message, excluding the authors star-reactions.</returns>
        Task<int> GetReactionCount(IUserMessage message, IEmote emote);

        /// <summary>
        /// Gets the appropriate star-emote for the given <paramref name="reactionCount"/>.
        /// </summary>
        /// <returns>A star-emote in string format</returns>
        string GetStarEmote(int reactionCount);

        /// <summary>
        /// Modifies a starboard entry.
        /// </summary>
        /// <param name="guild">Which guild's starboard to operate on</param>
        /// <param name="message">The message to modify</param>
        /// <param name="content">The content to modify with</param>
        /// <param name="embedColor">The color to modify the embed with</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task ModifyEntry(IGuild guild, IUserMessage message, string content, Color embedColor);

        /// <summary>
        /// Adds an <see cref="IUserMessage"/> to the starboard
        /// </summary>
        /// <param name="guild">The guild on which starboard to post</param>
        /// <param name="message">The message to add to the starboard</param>
        /// <param name="content">Meta-data that contains relevant information about the starred message</param>
        /// <param name="embed">The embed to include in the starboard-message</param>
        /// /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddToStarboard(IGuild guild, IUserMessage message, string content, Embed embed);
    }

    /// <inheritdoc />
    public class StarboardService : IStarboardService
    {
        private readonly DesignatedChannelService _designatedChannelService;
        private readonly IMessageRepository _messageRepository;
        private static readonly IReadOnlyDictionary<int, string> _emojis = new Dictionary<int, string>
        {
            { 20, "✨"},
            { 10, "💫" },
            { 5, "🌟" },
            { 0, "⭐" }
        }.OrderByDescending(k => k.Key).ToDictionary(k => k.Key, k => k.Value);

        public StarboardService(
            DesignatedChannelService designatedChannelService,
            IMessageRepository messageRepository)
        {
            _designatedChannelService = designatedChannelService;
            _messageRepository = messageRepository;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsOnStarboard(IMessage message)
        {
            var messageBrief = await _messageRepository.GetMessage(message.Id);
            return messageBrief?.StarboardEntryId != null;
        }

        private async Task<IUserMessage> GetStarboardEntry(IGuild guild, IMessage message)
        {
            var channel = await GetStarboardChannel(guild);
            var brief = await _messageRepository.GetMessage(message.Id);
            return await channel.GetMessageAsync(brief.StarboardEntryId.Value) as IUserMessage;
        }

        private async Task<ITextChannel> GetStarboardChannel(IGuild guild)
        {
            var starboardChannels = await _designatedChannelService
                .GetDesignatedChannels(guild, DesignatedChannelType.Starboard);

            return starboardChannels.First() as ITextChannel;
        }

        /// <inheritdoc />
        public async Task RemoveFromStarboard(IGuild guild, IMessage message)
        {
            using (var transaction = await _messageRepository.BeginMaintainTransactionAsync())
            {
                var messageBrief = await _messageRepository.GetMessage(message.Id);
                var channel = await GetStarboardChannel(guild);

                var msg = await channel.GetMessageAsync(messageBrief.StarboardEntryId.Value);
                if (msg != default)
                {
                    await channel.DeleteMessageAsync(messageBrief.StarboardEntryId.Value);
                    await _messageRepository.UpdateStarboardColumn(messageBrief.Id, null);
                }
                transaction.Commit();
            }

        }

        /// <inheritdoc />
        public bool IsStarEmote(IEmote emote)
            => emote.Name == _emojis.Values.Last();

        /// <inheritdoc />
        public async Task<int> GetReactionCount(IUserMessage message, IEmote emote)
        {
            var reactionUsers = await message
                .GetReactionUsersAsync(emote, 100)
                .FlattenAsync();
            //Ignore author reaction when counting stars
            return reactionUsers.Count(user => user.Id != message.Author.Id);
        }

        /// <inheritdoc />
        public bool IsAboveReactionThreshold(int reactionCount)
            => reactionCount >= 3;

        /// <inheritdoc />
        public string GetStarEmote(int reactionCount)
        {
            var emoteIndex = _emojis.Keys.First((val) => reactionCount >= val);
            return _emojis[emoteIndex];
        }

        /// <inheritdoc />
        public async Task AddToStarboard(IGuild guild, IUserMessage message, string content, Embed embed)
        {
            var starChannel = await GetStarboardChannel(guild);
            var starEntry = await starChannel.SendMessageAsync(content, false, embed);

            using (var transaction = await _messageRepository.BeginMaintainTransactionAsync())
            {
                if (await _messageRepository.GetMessage(message.Id) == null)
                {
                    var creationData = new MessageCreationData
                    {
                        Id = message.Id,
                        GuildId = guild.Id,
                        ChannelId = message.Channel.Id,
                        AuthorId = message.Author.Id,
                        Timestamp = message.Timestamp,
                    };
                    await _messageRepository.CreateAsync(creationData);
                }

                await _messageRepository.UpdateStarboardColumn(message.Id, starEntry.Id);
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task ModifyEntry(IGuild guild, IUserMessage message, string content, Color embedColor)
        {
            var starEntry = await GetStarboardEntry(guild, message);
            if (starEntry != default)
            {
                await starEntry.ModifyAsync(messageProps =>
                {
                    messageProps.Content = content;
                    messageProps.Embed = starEntry.Embeds
                                                  .First()
                                                  .ToEmbedBuilder()
                                                  .WithColor(embedColor)
                                                  .Build();
                });
            }
            else
            {
                await RemoveFromStarboard(guild, message);
            }
        }
    }
}
