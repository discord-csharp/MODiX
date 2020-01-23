using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modix.Data.Models.Moderation;
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

        public virtual GuildChannelEntity Channel { get; set; }

        [Required]
        public ulong AuthorId { get; set; }
        public virtual GuildUserEntity Author { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        public ulong? StarboardEntryId { get; set; }
    }

    public class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
    {
        public void Configure(EntityTypeBuilder<MessageEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasConversion<long>();
            builder
                .Property(x => x.GuildId)
                .HasConversion<long>();
            builder
                .Property(x => x.ChannelId)
                .HasConversion<long>();
            builder
                .Property(x => x.AuthorId)
                .HasConversion<long>();
            builder
                .Property(x => x.StarboardEntryId)
                .HasConversion<long>();
            builder
                .HasIndex(x => new { x.GuildId, x.AuthorId });

            builder
                .HasIndex(x => x.Timestamp);

            builder
                .HasOne(x => x.Author)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => new { x.GuildId, x.AuthorId });
        }
    }
}
