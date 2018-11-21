using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    public class MessageEntity
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

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageEntity>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.Id)
                .HasColumnType("numeric(20)");

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.GuildId)
                .HasColumnType("numeric(20)");

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.ChannelId)
                .HasColumnType("numeric(20)");

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.AuthorId)
                .HasColumnType("numeric(20)");

            modelBuilder.Entity<MessageEntity>()
                .HasIndex(x => new { x.GuildId, x.AuthorId });
        }
    }
}
