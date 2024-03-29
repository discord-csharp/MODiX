namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Defines the possible types of infractions that can be recorded for a user.
    /// </summary>
    public enum InfractionType
    {
        /// <summary>
        /// Describes an "infraction" that is really just a note for communication between moderators.
        /// </summary>
        Notice,
        /// <summary>
        /// Describes an infraction where the user was issued a warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Describes an infraction where the user was muted, preventing them from sending messages in text channels or speaking in voice channels.
        /// </summary>
        Mute,
        /// <summary>
        /// Describes an infraction where the user was banned from a guild.
        /// </summary>
        Ban,
    }
}
