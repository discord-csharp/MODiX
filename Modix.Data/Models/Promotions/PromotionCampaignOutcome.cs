namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Defines the possible types of outcomes of a promotion campaign.
    /// </summary>
    public enum PromotionCampaignOutcome
    {
        /// <summary>
        /// Describes a campaign that was accepted, for which the subject was promoted to the proposed rank.
        /// </summary>
        Accepted,
        /// <summary>
        /// Describes a campaign that was rejected, for which the subject was not promoted to the proposed rank.
        /// </summary>
        Rejected,
        /// <summary>
        /// Describes a campaign for which an error occurred during processing, that prevented the proposed promotion from being fully applied.
        /// </summary>
        Failed
    }
}
