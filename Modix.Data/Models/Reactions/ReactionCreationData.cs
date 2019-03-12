using System;

namespace Modix.Data.Models.Reactions
{
    /// <summary>
    /// Describes an operation to create a reaction.
    /// </summary>
    public class ReactionCreationData
    {
        /// <summary>
        /// The unique Discord snowflake ID of the guild in which the reaction occurred.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the channel in which the reaction occurred.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The Discord ID of the message that was reacted to.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the user who reacted.
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the emoji that was added as a reaction, if the emoji is a custom emoji.
        /// </summary>
        public ulong? EmojiId { get; set; }

        /// <summary>
        /// The name of the emoji.
        /// </summary>
        public string EmojiName { get; set; }

        internal ReactionEntity ToEntity()
            => new ReactionEntity()
            {
                GuildId = GuildId,
                ChannelId = ChannelId,
                MessageId = MessageId,
                UserId = UserId,
                EmojiId = EmojiId,
                EmojiName = EmojiName,
                Timestamp = DateTimeOffset.Now,
            };
    }
}
