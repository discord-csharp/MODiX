using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Emoji
{
    [Table("Emoji")]
    public class EmojiEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        public ulong MessageId { get; set; }

        [Required]
        public ulong UserId { get; set; }

        public ulong? EmojiId { get; set; }

        [Required]
        public string EmojiName { get; set; } = null!;

        [Required]
        public bool IsAnimated { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        [Required]
        public EmojiUsageType UsageType { get; set; }
    }

    public class EmojiEntityConfigurator
        : IEntityTypeConfiguration<EmojiEntity>
    {
        public void Configure(
            EntityTypeBuilder<EmojiEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.MessageId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.UserId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.EmojiId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.UsageType)
                .HasConversion<string>();

            entityTypeBuilder
                .HasIndex(x => x.GuildId);

            entityTypeBuilder
                .HasIndex(x => x.MessageId);

            entityTypeBuilder
                .HasIndex(x => x.UserId);

            entityTypeBuilder
                .HasIndex(x => x.EmojiId);

            entityTypeBuilder
                .HasIndex(x => x.EmojiName);

            entityTypeBuilder
                .HasIndex(x => x.Timestamp);

            entityTypeBuilder
                .HasIndex(x => x.UsageType);
        }
    }
}
