using System;

namespace Modix.Data.Models.Emoji
{
    /// <summary>
    /// Describes an operation to create an emoji record.
    /// </summary>
    public class EmojiCreationData
    {
        /// <summary>
        /// The unique Discord snowflake ID of the guild in which the emoji was used.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the channel in which the emoji was used.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The Discord ID of the message associated with the emoji.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the user who used the emoji.
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the emoji that was used, if the emoji is a custom emoji.
        /// </summary>
        public ulong? EmojiId { get; set; }

        /// <summary>
        /// The name of the emoji.
        /// </summary>
        public string EmojiName { get; set; } = null!;

        /// <summary>
        /// Indicates whether an emoji is animated.
        /// </summary>
        public bool IsAnimated { get; set; }

        /// <summary>
        /// The type of usage associated with the emoji.
        /// </summary>
        public EmojiUsageType UsageType { get; set; }

        internal EmojiEntity ToEntity()
            => new EmojiEntity()
            {
                GuildId = GuildId,
                ChannelId = ChannelId,
                MessageId = MessageId,
                UserId = UserId,
                EmojiId = EmojiId,
                EmojiName = EmojiName,
                IsAnimated = IsAnimated,
                UsageType = UsageType,
                Timestamp = DateTimeOffset.UtcNow,
            };

        internal EmojiEntity ToEntity(DateTimeOffset timestamp)
            => new EmojiEntity()
            {
                GuildId = GuildId,
                ChannelId = ChannelId,
                MessageId = MessageId,
                UserId = UserId,
                EmojiId = EmojiId,
                EmojiName = EmojiName,
                IsAnimated = IsAnimated,
                UsageType = UsageType,
                Timestamp = timestamp,
            };
    }
}
