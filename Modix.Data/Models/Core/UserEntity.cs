using System.ComponentModel.DataAnnotations;

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
    }
}
