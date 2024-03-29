namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Defines the possible types of actions that may be performed within the promotions system.
    /// </summary>
    public enum PromotionActionType
    {
        /// <summary>
        /// Describes an action where a promotion campaign was created.
        /// </summary>
        CampaignCreated,
        /// <summary>
        /// Describes an action where a promotion campaign was closed.
        /// </summary>
        CampaignClosed,
        /// <summary>
        /// Describes an action where a comment was added to an active promotion campaign.
        /// </summary>
        CommentCreated,
        /// <summary>
        /// Describes an action where a comment was modified in an active promotion campaign.
        /// </summary>
        CommentModified,
    }
}
