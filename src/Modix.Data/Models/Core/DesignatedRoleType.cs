namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the possible types designations that may be assigned to roles.
    /// </summary>
    public enum DesignatedRoleType
    {
        /// <summary>
        /// Defines a role that serves as a member of the rank hierarchy.
        /// </summary>
        Rank,
        /// <summary>
        /// Defines a role that is used by the moderation feature to mute users.
        /// </summary>
        ModerationMute,
        /// <summary>
        /// Defines a role whose mentionability is allowed throughout the guild.
        /// </summary>
        Pingable,
    }
}
