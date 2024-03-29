using System;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an operation to create a <see cref="PromotionCommentEntity"/>.
    /// </summary>
    public class PromotionCommentCreationData
    {
        /// <summary>
        /// See <see cref="PromotionCommentEntity.CampaignId"/>.
        /// </summary>
        public long CampaignId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Sentiment"/>.
        /// </summary>
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentEntity.Content"/>.
        /// </summary>
        public string? Content { get; set; } = null!;

        /// <summary>
        /// See <see cref="PromotionActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        public PromotionCommentEntity ToEntity()
            => new PromotionCommentEntity()
            {
                CampaignId = CampaignId,
                Sentiment = Sentiment,
                Content = Content,
                CreateAction = new PromotionActionEntity()
                {
                    GuildId = GuildId,
                    Created = DateTimeOffset.UtcNow,
                    Type = PromotionActionType.CommentCreated,
                    CreatedById = CreatedById,
                }
            };
    }
}
