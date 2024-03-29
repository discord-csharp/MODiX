using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a mapping that assigns an arbitrary designation to a particular role within a guild.
    /// </summary>
    [Table("DesignatedRoleMappings")]
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
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildRoleEntity.RoleId"/> value of <see cref="Role"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Role))]
        public ulong RoleId { get; set; }

        /// <summary>
        /// The role being designated by this mapping.
        /// </summary>
        [Required]
        public virtual GuildRoleEntity Role { get; set; } = null!;

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
        public virtual ConfigurationActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="DesignatedRoleMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity? DeleteAction { get; set; }
    }

    public class DesignatedRoleMappingEntityConfiguration
        : IEntityTypeConfiguration<DesignatedRoleMappingEntity>
    {
        public void Configure(
            EntityTypeBuilder<DesignatedRoleMappingEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.RoleId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DesignatedRoleMappingEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<DesignatedRoleMappingEntity>(x => x.DeleteActionId);
        }
    }
}
