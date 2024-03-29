namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Defines the possible opinions that a <see cref="PromotionCommentEntity"/> may express about a <see cref="PromotionCampaignEntity"/>.
    /// </summary>
    public enum PromotionSentiment
    {
        /// <summary>
        /// Describes a comment that does not express a specific opinion about a promotion campaign.
        /// </summary>
        Abstain,
        /// <summary>
        /// Describes a comment that approves of a promotion campaign.
        /// </summary>
        Approve,
        /// <summary>
        /// Describes a comment that opposes a promotion campaign.
        /// </summary>
        Oppose
    }
}
