using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core;

[Table("ConfigurationActions")]
public class ConfigurationActionEntity
{
    [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public ulong GuildId { get; set; }

    [Required]
    public ConfigurationActionType Type { get; set; }

    [Required]
    public DateTimeOffset Created { get; set; }

    [Required]
    public ulong CreatedById { get; set; }

    [Required]
    public virtual GuildUserEntity CreatedBy { get; set; } = null!;

    [ForeignKey(nameof(ClaimMapping))]
    public long? ClaimMappingId { get; set; }

    public ClaimMappingEntity? ClaimMapping { get; set; }

    [ForeignKey(nameof(DesignatedChannelMapping))]
    public long? DesignatedChannelMappingId { get; set; }

    public DesignatedChannelMappingEntity? DesignatedChannelMapping { get; set; }

    [ForeignKey(nameof(DesignatedRoleMapping))]
    public long? DesignatedRoleMappingId { get; set; }

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
