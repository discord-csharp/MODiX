using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a user of the application, that has previously joined a Discord guild managed by MODiX.
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// The unique identifier for this user within the Discord API.
        /// </summary>
        [Key, Required]
        public long Id { get; set; }

        /// <summary>
        /// The "username" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Username { get; set; }
        
        /// <summary>
        /// The "discriminator" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Discriminator { get; set; }

        /// <summary>
        /// The provided Nickname value of this user, within the Discord API.
        /// </summary>
        // TODO: Push this into a separate table, keyed by user and guild ID.
        public string Nickname { get; set; }

        /// <summary>
        /// A timestamp indicating the first time this user was observed by the aplication.
        /// </summary>
        [Required]
        public DateTimeOffset FirstSeen { get; set; }

        /// <summary>
        /// A timestamp indicating the most recent time this user was observed by the application.
        /// </summary>
        [Required]
        public DateTimeOffset LastSeen { get; set; }
    }
}
