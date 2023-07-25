using System;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Utilities
{
    public static class PromotionCampaignEntityExtensions
    {
        public static readonly TimeSpan CampaignAcceptCooldown = TimeSpan.FromHours(48);

        public static TimeSpan GetTimeUntilCampaignCanBeClosed(this PromotionCampaignSummary campaign)
            => campaign.CreateAction.Created.Add(CampaignAcceptCooldown) - DateTimeOffset.UtcNow;

        public static DateTimeOffset GetExpectedCampaignCloseTimeStamp(this PromotionCampaignSummary campaign)
            => campaign.CreateAction.Created.Add(CampaignAcceptCooldown);
    }
}
