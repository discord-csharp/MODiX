using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an action that was performed, that somehow changed the application's configuration.
    /// </summary>
    [Table("ConfigurationActions")]
    public class ConfigurationActionEntity
    {
        /// <summary>
        /// A unique identifier for this configuration action.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this configuration action applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The type of configuration action that was performed.
        /// </summary>
        [Required]
        public ConfigurationActionType Type { get; set; }

        /// <summary>
        /// A timestamp indicating when this configuration action was performed.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required]
        public ulong CreatedById { get; set; }

        /// <summary>
        /// The Discord user that performed this action.
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; } = null!;

        /// <summary>
        /// The <see cref="ClaimMappingEntity.Id"/> value (if any) of <see cref="ClaimMapping"/>.
        /// </summary>
        [ForeignKey(nameof(ClaimMapping))]
        public long? ClaimMappingId { get; set; }

        /// <summary>
        /// The claim mapping that was affected by this action, if any.
        /// </summary>
        public ClaimMappingEntity? ClaimMapping { get; set; }

        /// <summary>
        /// The <see cref="DesignatedChannelMappingEntity.Id"/> value (if any) of <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        [ForeignKey(nameof(DesignatedChannelMapping))]
        public long? DesignatedChannelMappingId { get; set; }

        /// <summary>
        /// The designated channel mapping that was affected by this action, if any.
        /// </summary>
        public DesignatedChannelMappingEntity? DesignatedChannelMapping { get; set; }

        /// <summary>
        /// The <see cref="DesignatedRoleMappingEntity.Id"/> value (if any) of <see cref="DesignatedRoleMappingEntity"/>.
        /// </summary>
        [ForeignKey(nameof(DesignatedRoleMapping))]
        public long? DesignatedRoleMappingId { get; set; }

        /// <summary>
        /// The designated role mapping that was affected by this action, if any.
        /// </summary>
        public DesignatedRoleMappingEntity? DesignatedRoleMapping { get; set; }
    }

    public class ConfigurationActionEntityConfigurator
        : IEntityTypeConfiguration<ConfigurationActionEntity>
    {
        public void Configure(
            EntityTypeBuilder<ConfigurationActionEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.CreatedById)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });
        }
    }
}
