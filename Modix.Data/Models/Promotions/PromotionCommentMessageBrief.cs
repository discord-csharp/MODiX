using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a partial view of a <see cref="PromotionCommentEntity"/>, for use within the context of a projected <see cref="PromotionCommentMessageEntity"/>.
    /// </summary>
    public class PromotionCommentMessageBrief
    {
        /// <summary>
        /// See <see cref="PromotionCommentEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// See <see cref="PromotionCommentEntity.Campaign"/>.
        /// </summary>
        public PromotionCampaignBrief Campaign { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        [ExpansionExpression]
        internal static Expression<Func<PromotionCommentEntity, PromotionCommentMessageBrief>> FromEntityProjection
            = entity => new PromotionCommentMessageBrief
            {
                Id = entity.Id,
                Campaign = (entity.Campaign == null)
                    ? null
                    : entity.Campaign.Project(PromotionCampaignBrief.FromEntityProjection),
                Sentiment = entity.Sentiment,
                Content = entity.Content,
            };
    }
}
