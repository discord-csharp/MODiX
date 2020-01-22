using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    public class GuildChannelEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ChannelId { get; set; }

        public ulong GuildId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public ICollection<DesignatedChannelMappingEntity> DesignatedChannelMappings { get; set; }
            = new HashSet<DesignatedChannelMappingEntity>();
    }

    public class GuildChannelEntityConfiguration : IEntityTypeConfiguration<GuildChannelEntity>
    {
        public void Configure(EntityTypeBuilder<GuildChannelEntity> builder)
        {
            builder
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            builder
                .Property(x => x.GuildId)
                .HasConversion<long>();
        }
    }
}
