using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of an <see cref="InfractionEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class InfractionBrief
    {
        /// <summary>
        /// See <see cref="InfractionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// See <see cref="InfractionEntity.RescindReason"/>.
        /// </summary>
        public string? RescindReason { get; set; } = null!;

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; } = null!;

        [ExpansionExpression]
        internal static Expression<Func<InfractionEntity, InfractionBrief>> FromEntityProjection
            = entity => new InfractionBrief()
            {
                Id = entity.Id,
                Type = entity.Type,
                Reason = entity.Reason,
                RescindReason = entity.RescindReason,
                Duration = entity.Duration,
                Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection)
            };
    }
}
