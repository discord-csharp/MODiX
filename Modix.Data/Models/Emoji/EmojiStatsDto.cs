using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Emoji
{
    internal class EmojiStatsDto
    {
        public ulong? EmojiId { get; set; }

        public string EmojiName { get; set; } = null!;

        public bool IsAnimated { get; set; }

        public int Rank { get; set; }

        public int Uses { get; set; }
    }

    internal class EmojiStatsDtoConfiguration
        : IEntityTypeConfiguration<EmojiStatsDto>
    {
        public void Configure(
                EntityTypeBuilder<EmojiStatsDto> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type EmojiStatsDto: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
