using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    [Table("Messages")]
    public class MessageEntity
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        public virtual GuildChannelEntity Channel { get; set; } = null!;

        [Required]
        public ulong AuthorId { get; set; }
        public virtual GuildUserEntity Author { get; set; } = null!;

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        public ulong? StarboardEntryId { get; set; }
    }

    public class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
    {
        public void Configure(
            EntityTypeBuilder<MessageEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .HasKey(x => x.Id);

            entityTypeBuilder
                .Property(x => x.Id)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.AuthorId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.StarboardEntryId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasIndex(x => new { x.GuildId, x.AuthorId });

            entityTypeBuilder
                .HasIndex(x => x.Timestamp);

            entityTypeBuilder
                .HasIndex(x => new { x.ChannelId, x.Timestamp, x.AuthorId });

            entityTypeBuilder
                .HasOne(x => x.Author)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => new { x.GuildId, x.AuthorId });
        }
    }
}
