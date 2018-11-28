using System.ComponentModel;

namespace Modix.Data.Models.Mentions
{
    /// <summary>
    /// Used to describe whether a role is mentionable and, if so, what interfaces can be used to mention the role.
    /// </summary>
    public enum MentionabilityType
    {
        /// <summary>
        /// Indicates that the role cannot be mentioned.
        /// </summary>
        [Description("Cannot be mentioned.")]
        NotMentionable,
        /// <summary>
        /// Indicates that the role can only be mentioned using a MODiX command.
        /// </summary>
        [Description("Mentionable using a MODiX command.")]
        ModixCommand,
        /// <summary>
        /// Indicates that the role can be mentioned either using the Discord "@" syntax or using a MODiX command.
        /// </summary>
        [Description("Mentionable using both the Discord \"@\" syntax and a MODiX command.")]
        DiscordSyntaxAndModixCommand,
    }
}
