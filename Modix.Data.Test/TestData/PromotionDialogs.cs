using System.Collections.Generic;
using System.Linq;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Test.TestData
{
    public static class PromotionDialogs
    {
        public static readonly IEnumerable<PromotionDialogEntity> Entities
            = new []
            {
                new PromotionDialogEntity {CampaignId = 1, MessageId = 12345},
                new PromotionDialogEntity {CampaignId = 2, MessageId = 2345},
                new PromotionDialogEntity {CampaignId = 3, MessageId = 345},
                new PromotionDialogEntity {CampaignId = 4, MessageId = 45}
            };

        public static PromotionDialogEntity Clone(this PromotionDialogEntity entity)
            => new PromotionDialogEntity
            {
                MessageId = entity.MessageId,
                CampaignId = entity.CampaignId
            };

        public static IEnumerable<PromotionDialogEntity> Clone(this IEnumerable<PromotionDialogEntity> entities)
            => entities.Select(Clone);

        public static readonly IEnumerable<PromotionDialogEntity> NewEntities
            = new []
            {
                new PromotionDialogEntity {CampaignId = 5, MessageId = 54321},
                new PromotionDialogEntity { CampaignId = 6, MessageId = 541532 }
            };

        public static readonly IEnumerable<PromotionDialogEntity> DeleteExistingEntities
            = new []
            {
                new PromotionDialogEntity {CampaignId = 1, MessageId = 12345},
                new PromotionDialogEntity {CampaignId = 2, MessageId = 2345},
            };
        public static readonly IEnumerable<PromotionDialogEntity> DeleteNonExistingEntities
            = new []
            {
                new PromotionDialogEntity {CampaignId = 0, MessageId = 9876},
                new PromotionDialogEntity {CampaignId = 0, MessageId = 6543}
            };
    }
}
