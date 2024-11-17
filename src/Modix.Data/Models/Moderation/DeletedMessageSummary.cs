using System;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation;

public class DeletedMessageSummary
{
    public ulong MessageId { get; set; }
    public ulong GuildId { get; set; }
    public GuildChannelBrief Channel { get; set; } = null!;
    public GuildUserBrief Author { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
    public GuildUserBrief CreatedBy { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public long? BatchId { get; set; }
    public DeletedMessageBatchBrief? Batch { get; set; } = null!;
}
