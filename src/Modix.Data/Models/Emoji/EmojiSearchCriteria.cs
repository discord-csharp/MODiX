using System.Linq;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Emoji
{
    /// <summary>
    /// Describes an operation to search for emoji.
    /// </summary>
    public class EmojiSearchCriteria
    {
        /// <summary>
        /// The unique Discord snowflake ID of the guild in which the emoji was used.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the channel in which the emoji was used.
        /// </summary>
        public ulong? ChannelId { get; set; }

        /// <summary>
        /// The Discord ID of the message associated with the emoji.
        /// </summary>
        public ulong? MessageId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the user who used the emoji.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the emoji that was used, if the emoji is a custom emoji.
        /// </summary>
        public ulong? EmojiId { get; set; }

        /// <summary>
        /// The name of the emoji.
        /// </summary>
        public string? EmojiName { get; set; }

        /// <summary>
        /// Indicates whether the emoji is animated.
        /// </summary>
        public bool? IsAnimated { get; set; }

        /// <summary>
        /// A range of dates of when the emoji was used.
        /// </summary>
        public DateTimeOffsetRange? TimestampRange { get; set; }

        /// <summary>
        /// The type of usage associated with the emoji.
        /// </summary>
        public EmojiUsageType? UsageType { get; set; }
    }

    internal static class EmojiSearchCriteriaExtensions
    {
        public static IQueryable<EmojiEntity> FilterBy(this IQueryable<EmojiEntity> query, EmojiSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId!.Value,
                    criteria.GuildId != null)
                .FilterBy(
                    x => x.ChannelId == criteria.ChannelId!.Value,
                    criteria.ChannelId != null)
                .FilterBy(
                    x => x.MessageId == criteria.MessageId!.Value,
                    criteria.MessageId != null)
                .FilterBy(
                    x => x.UserId == criteria.UserId!.Value,
                    criteria.UserId != null)
                .FilterBy(
                    x => x.EmojiId == criteria.EmojiId!.Value,
                    criteria.EmojiId != null)
                .FilterBy(
                    x => x.EmojiName == criteria.EmojiName,
                    criteria.EmojiName != null)
                .FilterBy(
                    x => x.IsAnimated == criteria.IsAnimated,
                    criteria.IsAnimated != null)
                .FilterBy(
                    x => x.Timestamp >= criteria.TimestampRange!.Value.From!.Value,
                    criteria.TimestampRange != null
                    && criteria.TimestampRange.Value.From != null)
                .FilterBy(
                    x => x.Timestamp <= criteria.TimestampRange!.Value.To!.Value,
                    criteria.TimestampRange != null
                    && criteria.TimestampRange.Value.To != null)
                .FilterBy(
                    x => x.UsageType == criteria.UsageType!.Value,
                    criteria.UsageType != null);
    }
}
