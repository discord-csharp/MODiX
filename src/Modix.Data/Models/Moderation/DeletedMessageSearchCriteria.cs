namespace Modix.Data.Models.Moderation;

public class DeletedMessageSearchCriteria
{
    public ulong? GuildId { get; set; }
    public ulong? ChannelId { get; set; }
    public string? Channel { get; set; }
    public ulong? AuthorId { get; set; }
    public string? Author { get; set; }
    public ulong? CreatedById { get; set; }
    public string? CreatedBy { get; set; }
    public string? Content { get; set; }
    public string? Reason { get; set; }
    public long? BatchId { get; set; }
}
