using System.Linq;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Utilities
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Get the total count of comments that were not abstaining
        /// </summary>
        public static int GetTotalVotes(this PromotionCampaignSummary campaign)
        {
            return campaign.CommentCounts
                .Where(x => x.Key != PromotionSentiment.Abstain)
                .Sum(x => x.Value);
        }

        /// <summary>
        /// Get the percentage of votes approving of this promotion.
        /// </summary>
        public static double GetApprovalPercentage(this PromotionCampaignSummary campaign)
        {
            var totalApprovals = campaign.CommentCounts
                .Where(x => x.Key == PromotionSentiment.Approve)
                .Sum(x => x.Value);

            if (totalApprovals < 1) return 0;

            double totalVotes = GetTotalVotes(campaign);

            return totalApprovals / totalVotes;
        }
    }
}
