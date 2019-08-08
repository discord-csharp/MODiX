﻿namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Defines the possible types of moderation actions that a staff member can perform.
    /// </summary>
    public enum ModerationActionType
    {
        /// <summary>
        /// Describes a moderation action where an infraction was recorded upon a user.
        /// </summary>
        InfractionCreated,
        /// <summary>
        /// Describes a moderation action where a previous infraction was rescinded from a user.
        /// </summary>
        InfractionRescinded,
        /// <summary>
        /// Describes a moderation action where a previous infraction was (soft) deleted.
        /// </summary>
        InfractionDeleted,
        /// <summary>
        /// Describes a moderation action where a message was automatically deleted.
        /// </summary>
        MessageDeleted,
        /// <summary>
        /// Describes a moderation action where a sequence of messages were deleted simultaneously.
        /// </summary>
        MessageBatchDeleted,
        /// <summary>
        /// Describes a moderation action where a previous, (soft) deleted infraction was restored to an active status.
        /// </summary>
        InfractionRestored,
    }
}
