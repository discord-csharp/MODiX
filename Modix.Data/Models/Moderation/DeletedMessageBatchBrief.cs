using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of a <see cref="DeletedMessageBatchEntity"/> for use within the context of another projected model.
    /// </summary>
    public class DeletedMessageBatchBrief
    {
        /// <summary>
        /// See <see cref="DeletedMessageBatchEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageBatchEntity.CreateAction"/>.
        /// </summary>
        public ModerationActionBrief CreateAction { get; set; } = null!;

        [ExpansionExpression]
        internal static Expression<Func<DeletedMessageBatchEntity, DeletedMessageBatchBrief>> FromEntityProjection
            = entity => new DeletedMessageBatchBrief()
            {
                Id = entity.Id,
                CreateAction = entity.CreateAction.Project(ModerationActionBrief.FromEntityProjection),
            };
    }
}
