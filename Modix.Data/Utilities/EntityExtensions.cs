using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Utilities
{
    public static class EntityExtensions
    {
        public static int GetTotalVotes(this PromotionCampaignSummary campaign)
        {
            return campaign.CommentCounts
                .Select(x => x.Value)
                .Sum();
        }

        public static double GetApprovalPercentage(this PromotionCampaignSummary campaign)
        {
            var totalApprovals = campaign.CommentCounts
                .Where(x => x.Key == PromotionSentiment.Approve)
                .Select(x => x.Value)
                .Sum();

            double totalVotes = GetTotalVotes(campaign);

            return totalApprovals / totalVotes;
        }
    }
}
