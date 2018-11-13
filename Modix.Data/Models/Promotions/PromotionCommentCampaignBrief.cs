using System;
using System.Linq.Expressions;

using Modix.Data.Projectables;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a partial view of a <see cref="PromotionActionEntity"/>, for use within the context of a projected <see cref="PromotionCampaignEntity"/>.
    /// </summary>
    public class PromotionCommentCampaignBrief
    {
        /// <summary>
        /// See <see cref="PromotionCommentEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.CreateAction"/>.
        /// </summary>
        public PromotionActionBrief CreateAction { get; set; }

        internal static Expression<Func<PromotionCommentEntity, PromotionCommentCampaignBrief>> FromEntityProjection
            = entity => new PromotionCommentCampaignBrief()
            {
                Id = entity.Id,
                Sentiment = entity.Sentiment,
                Content = entity.Content,
                CreateAction = entity.CreateAction.Project(PromotionActionBrief.FromEntityProjection)
            };
    }
}
