using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationActionSummary
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief CreatedBy { get; set; } = null!;

        /// <summary>
        /// See <see cref="ModerationActionEntity.Infraction"/>.
        /// </summary>
        public InfractionBrief? Infraction { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.DeletedMessage"/>.
        /// </summary>
        public DeletedMessageBrief? DeletedMessage { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.OriginalInfractionReason"/>.
        /// </summary>
        public string? OriginalInfractionReason { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageBatchEntity.DeletedMessages"/>.
        /// </summary>
        public IReadOnlyCollection<DeletedMessageBrief>? DeletedMessages { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<ModerationActionEntity, ModerationActionSummary>> FromEntityProjection
            = entity => new ModerationActionSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy.Project(GuildUserBrief.FromEntityProjection),
                Infraction = (entity.Infraction == null)
                    ? null
                    : entity.Infraction.Project(InfractionBrief.FromEntityProjection),
                DeletedMessage = (entity.DeletedMessage == null)
                    ? null
                    : entity.DeletedMessage.Project(DeletedMessageBrief.FromEntityProjection),
                DeletedMessages = (entity.DeletedMessageBatch == null)
                    ? null
                    : entity.DeletedMessageBatch.DeletedMessages.Select(x => x.Project(DeletedMessageBrief.FromEntityProjection)).ToArray(),
                OriginalInfractionReason = entity.OriginalInfractionReason,
            };
    }
}
