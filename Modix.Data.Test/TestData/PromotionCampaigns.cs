using System.Collections.Generic;
using System.Linq;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Test.TestData
{
    public static class PromotionCampaigns
    {
        public static readonly IEnumerable<PromotionCampaignEntity> Entities
            = new[]
            {
                new PromotionCampaignEntity { Id = 1 },
                new PromotionCampaignEntity { Id = 2 },
                new PromotionCampaignEntity { Id = 3 },
                new PromotionCampaignEntity { Id = 4 },
            };

        public static PromotionCampaignEntity Clone(this PromotionCampaignEntity entity)
            => new PromotionCampaignEntity
            {
                Id = entity.Id
            };

        public static IEnumerable<PromotionCampaignEntity> Clone(this IEnumerable<PromotionCampaignEntity> entities)
            => entities.Select(Clone);

    }
}
