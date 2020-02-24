using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    public class MessageCountPerChannel
    {
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; } = null!;
        public int MessageCount { get; set; }
    }

    public class MessageCountPerChannelConfiguration
        : IEntityTypeConfiguration<MessageCountPerChannel>
    {
        public void Configure(
                EntityTypeBuilder<MessageCountPerChannel> entityTypeBuilder)
            => entityTypeBuilder
                .HasNoKey()
                // Workaround until .NET 5: https://github.com/dotnet/efcore/issues/19972
                .ToView("No table or view exists for entity type MessageCountPerChannel: This type can only be queried with raw SQL (.FromSqlXXX())");
    }
}
