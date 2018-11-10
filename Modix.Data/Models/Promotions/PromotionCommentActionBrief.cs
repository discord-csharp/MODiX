using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a partial view of a <see cref="PromotionActionEntity"/>, for use within the context of a projected <see cref="PromotionActionEntity"/>.
    /// </summary>
    public class PromotionCommentActionBrief
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
        internal static Expression<Func<PromotionCommentEntity, PromotionCommentActionBrief>> FromEntityProjection
            = entity => new PromotionCommentActionBrief()
            {
                Id = entity.Id,
                Campaign = entity.Campaign.Project(PromotionCampaignBrief.FromEntityProjection),
                // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                //Sentiment = entity.Sentiment,
                Sentiment = Enum.Parse<PromotionSentiment>(entity.Sentiment.ToString()),
                Content = entity.Content
            };
    }
}
