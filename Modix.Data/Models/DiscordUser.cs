using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    /// <summary>
    /// Describes a Discord user that has at least once joined the Discord guild managed by MODiX.
    /// </summary>
    public class DiscordUser
    {
        /// <summary>
        /// A unique identifier for this user.
        /// </summary>
        [Key]
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// The unique identifier for this user within the Discord API.
        /// </summary>
        [NotMapped]
        public ulong DiscordUserId
        {
            get => (ulong)UserId;
            set => UserId = (long)value;
        }

        /// <summary>
        /// The "username" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Username { get; set; }
        
        /// <summary>
        /// The provided Nickname value of this server, within the Discord API.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The "discriminator" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public uint Discriminator { get; set; }
        
        /// <summary>
        /// A timestamp indictating when this Discord User account was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

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
        
        /// <summary>
        /// A url link to Avatar image
        /// </summary>
        public string AvatarUrl { get; set; }
        
        /// <summary>
        /// A boolean indicating whether or not if this user is a bot. 
        /// </summary>
        public bool IsBot { get; set; }
    }
}
