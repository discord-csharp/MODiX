namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the possible types of moderation actions that can be performed.
    /// </summary>
    public enum ConfigurationActionType
    {
        /// <summary>
        /// Describes an action where a claim mapping was created.
        /// </summary>
        ClaimMappingCreated,
        /// <summary>
        /// Describes an action where a claim mapping was rescinded.
        /// </summary>
        ClaimMappingDeleted,
        /// <summary>
        /// Describes an action where a moderation mute role mapping was created.
        /// </summary>
        ModerationMuteRoleMappingCreated,
        /// <summary>
        /// Describes an action where a moderation mute role mapping was deleted.
        /// </summary>
        ModerationMuteRoleMappingDeleted,
        /// <summary>
        /// Describes an action where a moderation log channel mapping was created.
        /// </summary>
        ModerationLogChannelMappingCreated,
        /// <summary>
        /// Describes an action where a moderation log channel mapping was deleted.
        /// </summary>
        ModerationLogChannelMappingDeleted
    }
}
