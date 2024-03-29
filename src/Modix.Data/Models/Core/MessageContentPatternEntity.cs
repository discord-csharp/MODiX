using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    [Table("MessageContentPatterns")]
    public class MessageContentPatternEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Pattern { get; set; } = null!;

        public MessageContentPatternType PatternType { get; set; }

        public ulong GuildId { get; set; }
    }

    public class MessageContentPatternEntityConfiguration
        : IEntityTypeConfiguration<MessageContentPatternEntity>
    {
        public void Configure(
            EntityTypeBuilder<MessageContentPatternEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.PatternType)
                .HasConversion<string>();

            entityTypeBuilder
                .HasIndex(p => new { p.GuildId, p.Pattern })
                .IsUnique();
        }
    }
}
