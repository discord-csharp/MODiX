namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the possible types designations that may be assigned to channels.
    /// </summary>
    public enum DesignatedChannelType
    {
        /// <summary>
        /// Defines a channel that logs actions performed by the moderation feature.
        /// </summary>
        ModerationLog,
        /// <summary>
        /// Defines a channel that logs modified and deleted messages.
        /// </summary>
        MessageLog,
        /// <summary>
        /// Defines a channel that logs actions performed by the promotions feature.
        /// </summary>
        PromotionLog,
        /// <summary>
        /// Defines a channel to send promotion campaign creation/closing notifications.
        /// </summary>
        PromotionNotifications,
        /// <summary>
        /// Defines a channel that is not subject to auto-moderation behaviors of the moderation feature.
        /// </summary>
        Unmoderated,
        /// <summary>
        /// Defines a channel to which starred messages are sent.
        /// </summary>
        Starboard,
        /// <summary>
        /// Defines a channel that should be included when calculating user participation.
        /// </summary>
        CountsTowardsParticipation,
        /// <summary>
        /// Defines a channel where messages, if starred, are not sent to the starboard
        /// </summary>
        IgnoredFromStarboard,
    }
}
