using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a partial view of a <see cref="PromotionCampaignEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class PromotionCampaignBrief
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.TargetRole"/>.
        /// </summary>
        public GuildRoleBrief TargetRole { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Outcome"/>.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }
    }
}
