using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes configuration information for a guild managed by the Moderation feature
    /// </summary>
    public class ModerationConfigEntity
    {
        /// <summary>
        /// A timestamp indicating when this entity was created, for auditing purposes.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The unique identifier, within the Discord API, of the guild for to which this configuration applies.
        /// </summary>
        [Required, Key]
        public long GuildId { get; set; }

        /// <summary>
        /// The unique identifier, within the Discord API, of the role that is used to mute users, within this configured guild.
        /// </summary>
        [Required]
        public long MuteRoleId { get; set; }
    }
}
