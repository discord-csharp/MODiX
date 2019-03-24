using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Reactions
{
    [Table("Reactions")]
    internal class ReactionEntity
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
        public string EmojiName { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReactionEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder.Entity<ReactionEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder.Entity<ReactionEntity>()
                .Property(x => x.MessageId)
                .HasConversion<long>();

            modelBuilder.Entity<ReactionEntity>()
                .Property(x => x.UserId)
                .HasConversion<long>();

            modelBuilder.Entity<ReactionEntity>()
                .Property(x => x.EmojiId)
                .HasConversion<long>();

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.GuildId);

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.MessageId);

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.UserId);

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.EmojiId);

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.EmojiName);

            modelBuilder.Entity<ReactionEntity>()
                .HasIndex(x => x.Timestamp);
        }
    }
}
