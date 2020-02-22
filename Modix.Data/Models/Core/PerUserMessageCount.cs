using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    public class PerUserMessageCount
    {
        public string Username { get; set; } = null!;

        public string Discriminator { get; set; } = null!;

        public int Rank { get; set; }

        public int MessageCount { get; set; }

        public bool IsCurrentUser { get; set; }
    }

    public class PerUserMessageCountConfiguration
        : IEntityTypeConfiguration<PerUserMessageCount>
    {
        public void Configure(
                EntityTypeBuilder<PerUserMessageCount> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type PerUserMessageCount: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
