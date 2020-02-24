using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a Discord guild role, tracked by the application. 
    /// Tracking this information locally, helps us avoid calls to the Discord API,
    /// and to keep a history for roles that have been deleted from the Discord API.
    /// </summary>
    [Table("GuildRoles")]
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
        public string Name { get; set; } = null!;

        /// <summary>
        /// The position of this role within its owner guild's hierarchy.
        /// </summary>
        [Required]
        public int Position { get; set; }
    }

    public class GuildRoleEntityConfiguration
        : IEntityTypeConfiguration<GuildRoleEntity>
    {
        public void Configure(
            EntityTypeBuilder<GuildRoleEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.RoleId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();
        }
    }
}
