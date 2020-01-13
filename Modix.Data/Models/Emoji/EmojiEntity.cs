using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

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

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.MessageId)
                .HasConversion<long>();

            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.UserId)
                .HasConversion<long>();

            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.EmojiId)
                .HasConversion<long>();

            modelBuilder.Entity<EmojiEntity>()
                .Property(x => x.UsageType)
                .HasConversion<string>();

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.GuildId);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.MessageId);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.EmojiId);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.EmojiName);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.Timestamp);

            modelBuilder.Entity<EmojiEntity>()
                .HasIndex(x => x.UsageType);
        }
    }
}
