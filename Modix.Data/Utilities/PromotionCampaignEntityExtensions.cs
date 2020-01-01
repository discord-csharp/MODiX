using System;
using System.Linq;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Utilities
{
    public static class PromotionCampaignEntityExtensions
    {
        public static readonly TimeSpan CampaignAcceptCooldown = TimeSpan.FromHours(48);

        /// <summary>
        /// Get the total count of comments that were not abstaining
        /// </summary>
        public static int GetTotalVotes(this PromotionCampaignSummary campaign)
        {
            return campaign.CommentCounts
                .Where(x => x.Key != PromotionSentiment.Abstain)
                .Sum(x => x.Value);
        }

        public static int GetNumberOfApprovals(this PromotionCampaignSummary campaign)
        {
            return campaign.CommentCounts
                .Where(x => x.Key == PromotionSentiment.Approve)
                .Sum(x => x.Value);
        }

        public static int GetNumberOfOppositions(this PromotionCampaignSummary campaign)
        {
            return campaign.CommentCounts
                .Where(x => x.Key == PromotionSentiment.Oppose)
                .Sum(x => x.Value);
        }

        public static TimeSpan GetTimeUntilCampaignCanBeClosed(this PromotionCampaignSummary campaign)
        {
            return campaign.CreateAction.Created.Add(CampaignAcceptCooldown) - DateTimeOffset.Now;
        }
    }
}
