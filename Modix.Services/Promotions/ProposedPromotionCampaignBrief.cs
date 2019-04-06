using Modix.Data.Models.Core;

namespace Modix.Services.Promotions
{
    public class ProposedPromotionCampaignBrief
    {
        public ulong NominatingUserId { get; set; }

        public GuildRoleBrief TargetRankRole { get; set; }
    }
}
