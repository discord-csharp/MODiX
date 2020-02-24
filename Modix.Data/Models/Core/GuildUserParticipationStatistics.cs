using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    public class GuildUserParticipationStatistics
    {
        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public int Rank { get; set; }

        public decimal AveragePerDay { get; set; }

        public int Percentile { get; set; }
    }

    public class GuildUserParticipationStatisticsConfiguration
        : IEntityTypeConfiguration<GuildUserParticipationStatistics>
    {
        public void Configure(
                EntityTypeBuilder<GuildUserParticipationStatistics> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type GuildUserParticipationStatistics: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
