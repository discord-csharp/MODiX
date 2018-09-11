using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;
using Modix.Data.Projectables;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a partial view of a <see cref="PromotionActionEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class PromotionActionBrief
    {
        /// <summary>
        /// See <see cref="PromotionActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief CreatedBy { get; set; }

        internal static Expression<Func<PromotionActionEntity, PromotionActionBrief>> FromEntityProjection
            = entity => new PromotionActionBrief()
            {
                Id = entity.Id,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy.Project(GuildUserBrief.FromEntityProjection)
            };
    }
}
