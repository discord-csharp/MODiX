using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

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
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        /// <summary>
        /// The "username" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// The "discriminator" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Discriminator { get; set; } = null!;

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserEntity>()
                .Property(x => x.Id)
                .HasConversion<long>();
        }
    }
}
