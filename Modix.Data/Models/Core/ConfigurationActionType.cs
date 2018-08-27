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
        /// Describes an action where a designated channel mapping was created.
        /// </summary>
        DesignatedChannelMappingCreated,
        /// <summary>
        /// Describes an action where a designated channel mapping was deleted.
        /// </summary>
        DesignatedChannelMappingDeleted,
        /// <summary>
        /// Describes an action where a designated role mapping was created.
        /// </summary>
        DesignatedRoleMappingCreated,
        /// <summary>
        /// Describes an action where a designated role mapping was deleted.
        /// </summary>
        DesignatedRoleMappingDeleted
    }
}
