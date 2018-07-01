using System;
using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    /// <summary>
    /// Describes a Discord user that has at least once joined the Discord guild managed by MODiX.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique identifier for this user within the Discord API.
        /// </summary>
        [Key]
        [Required]
        public ulong Id { get; set; }

        /// <summary>
        /// The "username" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// The "discriminator" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public uint Discriminator { get; set; }

        /// <summary>
        /// A timestamp indicating the first time a message was received from this user.
        /// </summary>
        [Required]
        public DateTimeOffset FirstSeen { get; set; }

        /// <summary>
        /// A timestamp indicating the most recent time a message was received from this user.
        /// </summary>
        [Required]
        public DateTimeOffset LastSeen { get; set; }
    }
}
