using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    public class DesignatedChannelMappingEntity
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ulong GuildId { get; set; }

        [ForeignKey(nameof(Channel))]
        public ulong ChannelId { get; set; }

        public virtual GuildChannelEntity Channel { get; set; } = null!;

        public DesignatedChannelType Type { get; set; }

        public long CreateActionId { get; set; }

        public virtual ConfigurationActionEntity CreateAction { get; set; } = null!;

        public long? DeleteActionId { get; set; }

        public virtual ConfigurationActionEntity? DeleteAction { get; set; }
    }

    public class DesignatedChannelMappingEntityConfiguration : IEntityTypeConfiguration<DesignatedChannelMappingEntity>
    {
        public void Configure(EntityTypeBuilder<DesignatedChannelMappingEntity> builder)
        {
            builder
                .Property(x => x.Type)
                .HasConversion<string>();
            builder
                .Property(x => x.GuildId)
                .HasConversion<long>();
            builder
                .Property(x => x.ChannelId)
                .HasConversion<long>();
            builder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DesignatedChannelMappingEntity>(x => x.CreateActionId);
            builder
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<DesignatedChannelMappingEntity>(x => x.DeleteActionId);
        }
    }
}
