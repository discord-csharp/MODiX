namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Defines the possible types of moderation actions that a staff member can perform.
    /// </summary>
    public enum ModerationActionType
    {
        /// <summary>
        /// Describes a moderation action where an infraction was recorded upon a user.
        /// </summary>
        InfractionCreated = 0,
        /// <summary>
        /// Describs a moderation action where a previous infraction was rescinded from a user.
        /// </summary>
        InfractionRescinded = 1,
    }
}
