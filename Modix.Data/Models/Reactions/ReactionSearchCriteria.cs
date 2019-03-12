using System.Linq;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Reactions
{
    /// <summary>
    /// Describes an operation to search for reactions.
    /// </summary>
    public class ReactionSearchCriteria
    {
        /// <summary>
        /// The unique Discord snowflake ID of the guild in which the reaction occurred.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the channel in which the reaction occurred.
        /// </summary>
        public ulong? ChannelId { get; set; }

        /// <summary>
        /// The Discord ID of the message that was reacted to.
        /// </summary>
        public ulong? MessageId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the user who reacted.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the emoji that was added as a reaction, if the emoji is a custom emoji.
        /// </summary>
        public ulong? EmojiId { get; set; }

        /// <summary>
        /// The name of the emoji.
        /// </summary>
        public string EmojiName { get; set; }

        /// <summary>
        /// A range of dates of when the reaction occurred.
        /// </summary>
        public DateTimeOffsetRange? TimestampRange { get; set; }
    }

    internal static class ReactionSearchCriteriaExtensions
    {
        public static IQueryable<ReactionEntity> FilterBy(this IQueryable<ReactionEntity> query, ReactionSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId.Value,
                    criteria.GuildId != null)
                .FilterBy(
                    x => x.ChannelId == criteria.ChannelId.Value,
                    criteria.ChannelId != null)
                .FilterBy(
                    x => x.MessageId == criteria.MessageId.Value,
                    criteria.MessageId != null)
                .FilterBy(
                    x => x.UserId == criteria.UserId.Value,
                    criteria.UserId != null)
                .FilterBy(
                    x => x.EmojiId == criteria.EmojiId.Value,
                    criteria.EmojiId != null)
                .FilterBy(
                    x => x.EmojiName == criteria.EmojiName,
                    criteria.EmojiName != null)
                .FilterBy(
                    x => x.Timestamp >= criteria.TimestampRange.Value.From.Value,
                    criteria.TimestampRange != null
                    && criteria.TimestampRange.Value.From != null)
                .FilterBy(
                    x => x.Timestamp <= criteria.TimestampRange.Value.To.Value,
                    criteria.TimestampRange != null
                    && criteria.TimestampRange.Value.To != null);
    }
}
