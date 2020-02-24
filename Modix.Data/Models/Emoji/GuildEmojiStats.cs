using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Emoji
{
    public class GuildEmojiStats
    {
        public int UniqueEmojis { get; set; }

        public int TotalUses { get; set; }

        public DateTimeOffset OldestTimestamp { get; set; }
    }

    public class GuildEmojiStatsConfiguration
        : IEntityTypeConfiguration<GuildEmojiStats>
    {
        public void Configure(
                EntityTypeBuilder<GuildEmojiStats> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type GuildEmojiStats: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
