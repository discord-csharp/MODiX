using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    [Table("GuildChannels")]
    public class GuildChannelEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public ulong? ParentChannelId { get; set; }

        public ICollection<DesignatedChannelMappingEntity> DesignatedChannelMappings { get; set; }
            = new HashSet<DesignatedChannelMappingEntity>();
    }

    public class GuildChannelEntityConfiguration
        : IEntityTypeConfiguration<GuildChannelEntity>
    {
        public void Configure(
            EntityTypeBuilder<GuildChannelEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.ParentChannelId)
                .HasConversion<long>();
        }
    }
}
