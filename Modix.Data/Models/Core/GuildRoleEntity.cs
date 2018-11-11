using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a Discord guild role, tracked by the application. 
    /// Tracking this information locally, helps us avoid calls to the Discord API,
    /// and to keep a history for roles that have been deleted from the Discord API.
    /// </summary>
    public class GuildRoleEntity
    {
        /// <summary>
        /// The Discord snowflake ID of this role.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleId { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this role belongs.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The display name of the role.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The position of this role within its owner guild's hierarchy.
        /// </summary>
        [Required]
        public int Position { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<GuildRoleEntity>()
                .Property(x => x.RoleId)
                .HasConversion<long>();

            modelBuilder
                .Entity<GuildRoleEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();
        }
    }
}
