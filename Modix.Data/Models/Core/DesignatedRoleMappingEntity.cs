using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a mapping that assigns an arbitrary designation to a particular role within a guild.
    /// </summary>
    public class DesignatedRoleMappingEntity
    {
        /// <summary>
        /// A unique identifier for this mapping.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildRoleEntity.RoleId"/> value of <see cref="Role"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Role))]
        public long RoleId { get; set; }

        /// <summary>
        /// The role being designated by this mapping.
        /// </summary>
        [Required]
        public virtual GuildRoleEntity Role { get; set; }

        /// <summary>
        /// The type of designation being defined for this role.
        /// </summary>
        [Required]
        public DesignatedRoleType Type { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> that created this <see cref="DesignatedRoleMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="DesignatedRoleMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity DeleteAction { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DesignatedRoleMappingEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<DesignatedRoleMappingEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DesignatedRoleMappingEntity>(x => x.CreateActionId);

            modelBuilder
                .Entity<DesignatedRoleMappingEntity>()
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<DesignatedRoleMappingEntity>(x => x.DeleteActionId);
        }
    }
}
