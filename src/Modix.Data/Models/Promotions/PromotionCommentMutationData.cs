namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an operation to modify a <see cref="PromotionCommentEntity"/> object.
    /// </summary>
    public class PromotionCommentMutationData
    {
        /// <summary>
        /// See <see cref="PromotionCommentEntity.CampaignId"/>.
        /// </summary>
        public long CampaignId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Content"/>.
        /// </summary>
        public string? Content { get; set; } = null!;

        internal static PromotionCommentMutationData FromEntity(PromotionCommentEntity entity)
            => new PromotionCommentMutationData
            {
                CampaignId = entity.CampaignId,
                Sentiment = entity.Sentiment,
                Content = entity.Content,
            };

        internal PromotionCommentEntity ToEntity()
            => new PromotionCommentEntity
            {
                CampaignId = CampaignId,
                Sentiment = Sentiment,
                Content = Content,
            };
    }
}
