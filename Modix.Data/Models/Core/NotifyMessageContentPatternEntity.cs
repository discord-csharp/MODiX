using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Modix.Data.Models.Core
{
    [Table("ListeningMessagePatterns")]
    public class ListeningMessagePatternEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ulong GuildId { get; set; }

        [Required]
        public string Pattern { get; set; } = null!;

        public bool Disabled { get; set; }

        public ICollection<GuildUserEntity> Listeners { get; set; } = new HashSet<GuildUserEntity>();
    }

    public class NotifyMessageContentPatternEntityConfiguration
        : IEntityTypeConfiguration<ListeningMessagePatternEntity>
    {
        public void Configure(
            EntityTypeBuilder<ListeningMessagePatternEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasIndex(p => new { p.Pattern, p.GuildId })
                .IsUnique();

            entityTypeBuilder
                .HasMany(s => s.Listeners)
                .WithMany(s => s.ListeningMessagePatterns);
                
        }
    }
}
