using System;
using System.Linq;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="PromotionCommentEntity"/> entities within an <see cref="IPromotionCommentRepository"/>.
    /// </summary>
    public class PromotionCommentSearchCriteria
    {
        /// <summary>
        /// A <see cref="PromotionCommentEntity.CampaignId"/> value, defining the <see cref="PromotionCommentEntity"/> entities to be returned.
        /// </summary>
        public long? CampaignId { get; set; }

        /// <summary>
        /// A <see cref="PromotionCommentEntity.Sentiment"/> value, defining the <see cref="PromotionCommentEntity"/> entities to be returned.
        /// </summary>
        public PromotionSentiment? Sentiment { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="PromotionCommentEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.Created"/> value of <see cref="PromotionCommentEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="PromotionCommentEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.CreatedById"/> value of <see cref="PromotionCommentEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have a <see cref="PromotionCommentEntity.ModifyActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsModified { get; set; }
    }

    internal static class PromotionCommentSearchCriteriaExtensions
    {
        public static IQueryable<PromotionCommentEntity> FilterBy(this IQueryable<PromotionCommentEntity> query, PromotionCommentSearchCriteria criteria)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (criteria is null)
                return query;

            return query
                .FilterBy(
                    x => x.CampaignId == criteria.CampaignId!.Value,
                    !(criteria.CampaignId is null))
                .FilterBy(
                    x => x.Sentiment == criteria.Sentiment!.Value,
                    !(criteria.Sentiment is null))
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange!.Value.From!.Value,
                    !(criteria.CreatedRange?.From is null))
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange!.Value.To!.Value,
                    !(criteria.CreatedRange?.To is null))
                .FilterBy(
                    x => x.CreateAction.CreatedById == criteria.CreatedById,
                    !(criteria.CreatedById is null))
                .FilterBy(
                    x => (x.ModifyActionId != null) == criteria.IsModified,
                    criteria?.IsModified != null);
        }
    }
}
