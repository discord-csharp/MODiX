using System;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    [DebuggerDisplay("({Date}: {MessageCount})")]
    public class MessageCountByDate
    {
        public DateTime Date { get; set; }
        public int MessageCount { get; set; }
    }

    public class MessageCountByDateConfiguration
        : IEntityTypeConfiguration<MessageCountByDate>
    {
        public void Configure(
                EntityTypeBuilder<MessageCountByDate> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type MessageCountByDate: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
