using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCommentEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionCommentSummary
    {
        /// <summary>
        /// See <see cref="PromotionCommentEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// See <see cref="PromotionCommentEntity.Campaign"/>.
        /// </summary>
        public PromotionCampaignBrief? Campaign { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Content"/>.
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// See <see cref="PromotionCommentEntity.CreateAction"/>.
        /// </summary>
        public PromotionActionBrief? Created { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.ModifyAction"/>.
        /// </summary>
        public PromotionActionBrief? Modified { get; set; }

        [ExpansionExpression]
        internal static Expression<Func<PromotionCommentEntity, PromotionCommentSummary>> FromEntityProjection
            = entity => new PromotionCommentSummary
            {
                Id = entity.Id,
                Campaign = (entity.Campaign == null)
                    ? null
                    : entity.Campaign.Project(PromotionCampaignBrief.FromEntityProjection),
                Sentiment = entity.Sentiment,
                Content = entity.Content,
                Created = (entity.CreateAction == null)
                    ? null
                    : entity.CreateAction.Project(PromotionActionBrief.FromEntityProjection),
                Modified = (entity.ModifyAction == null)
                    ? null
                    : entity.ModifyAction.Project(PromotionActionBrief.FromEntityProjection),
            };
    }
}
