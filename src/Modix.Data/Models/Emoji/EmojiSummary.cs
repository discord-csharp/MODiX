using System;
using System.Linq.Expressions;
using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Emoji
{
    /// <summary>
    /// Provides a view of an emoji log record.
    /// </summary>
    public class EmojiSummary
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
        /// The emoji that was used.
        /// </summary>
        public EphemeralEmoji Emoji { get; set; } = null!;

        /// <summary>
        /// A timestamp indicating when the emoji was used.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The type of usage associated with the emoji.
        /// </summary>
        public EmojiUsageType UsageType { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<EmojiEntity, EmojiSummary>> FromEntityProjection
            = entity => new EmojiSummary()
            {
                GuildId = entity.GuildId,
                ChannelId = entity.ChannelId,
                MessageId = entity.MessageId,
                UserId = entity.UserId,
                Emoji = EphemeralEmoji.FromRawData(entity.EmojiName, entity.EmojiId, entity.IsAnimated),
                Timestamp = entity.Timestamp,
                UsageType = entity.UsageType,
            };
    }
}
