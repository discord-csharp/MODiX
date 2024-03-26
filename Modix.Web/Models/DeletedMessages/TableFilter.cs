namespace Modix.Web.Models.DeletedMessages;

public class TableFilter
{
    private string? _author;
    public string? Author
    {
        get => _author;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _author = null;
                AuthorId = null;
            }
            else if (ulong.TryParse(value, out var subjectId))
            {
                AuthorId = subjectId;
            }
            else
            {
                _author = value;
            }
        }
    }

    public ulong? AuthorId { get; private set; }

    private string? _createdBy;
    public string? CreatedBy
    {
        get => _createdBy;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _createdBy = null;
                CreatedById = null;
            }
            else if (ulong.TryParse(value, out var createdById))
            {
                CreatedById = createdById;
            }
            else
            {
                _createdBy = value;
            }
        }
    }

    public ulong? CreatedById { get; private set; }

    private string? _channel;
    public string? Channel
    {
        get => _channel;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _channel = null;
                ChannelId = null;
            }
            else if (ulong.TryParse(value, out var channelId))
            {
                ChannelId = channelId;
            }
            else
            {
                _channel = value;
            }
        }
    }

    public long? BatchId { get; set; }

    public ulong? ChannelId { get; private set; }
    public string? Content { get; set; }
    public string? Reason { get; set; }

    public void SetBatchId(string? batchId)
    {
        if (!long.TryParse(batchId, out var id))
        {
            BatchId = null;
            return;
        }

        BatchId = id;
    }
}
