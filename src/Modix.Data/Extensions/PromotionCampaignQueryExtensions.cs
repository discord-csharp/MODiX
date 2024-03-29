using System;
using System.Linq;
using System.Linq.Expressions;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Extensions
{
    public static class PromotionCampaignQueryExtensions
    {
        public static Expression<Func<PromotionCampaignEntity, bool>> IsAccepted()
        {
            return x => x.Outcome == PromotionCampaignOutcome.Accepted && x.CloseActionId != null;
        }

        public static IQueryable<PromotionCampaignEntity> WhereIsAccepted(this IQueryable<PromotionCampaignEntity> source)
        {
            return source.Where(IsAccepted());
        }
    }
}
