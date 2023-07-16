using System.Collections.Generic;
using System.Linq;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationActionEntity"/> entities within an <see cref="IModerationActionRepository"/>.
    /// </summary>
    public class ModerationActionSearchCriteria
    {
        /// <summary>
        /// A <see cref="ModerationActionEntity.GuildId"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A set of <see cref="ModerationActionEntity.Type"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public ModerationActionType[]? Types { get; set; }

        /// <summary>
        /// A range of <see cref="ModerationActionEntity.Created"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="ModerationActionEntity.CreatedById"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public ulong? CreatedById { get; set; }
    }

    internal static class ModerationActionQueryableExtensions
    {
        public static IQueryable<ModerationActionEntity> FilterBy(this IQueryable<ModerationActionEntity> query, ModerationActionSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId,
                    criteria.GuildId != null)
                .FilterBy(
                    x => criteria.Types!.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.Created >= criteria!.CreatedRange!.Value.From,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.Created <= criteria!.CreatedRange!.Value.To,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreatedById == criteria!.CreatedById,
                    criteria?.CreatedById != null);
    }
}
