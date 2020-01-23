using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    internal class MessageEntity
    {
        [Required]
        public ulong Id { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        public ulong AuthorId { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        public ulong? StarboardEntryId { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageEntity>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.Id)
                .HasConversion<long>();

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.AuthorId)
                .HasConversion<long>();

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.StarboardEntryId)
                .HasConversion<long>();

            modelBuilder.Entity<MessageEntity>()
                .HasIndex(x => new { x.GuildId, x.AuthorId });

            modelBuilder.Entity<MessageEntity>()
                .HasIndex(x => x.Timestamp);
        }
    }
}
