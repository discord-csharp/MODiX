using Modix.Data.Models.Moderation;

namespace Modix.Web.Models.Infractions;

public class TableFilter
{
    public long? Id => long.TryParse(IdString, out var id) ? id : null;
    public string? IdString { get; set; }

    public InfractionType? Type { get; set; }
    public InfractionType[]? Types => Type is not null ? new[] { Type.Value } : null;

    private string? _subject;
    public string? Subject
    {
        get => _subject;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _subject = null;
                SubjectId = null;
            }
            else if (ulong.TryParse(value, out var subjectId))
            {
                SubjectId = subjectId;
            }
            else
            {
                _subject = value;
            }
        }
    }

    public ulong? SubjectId { get; private set; }

    private string? _creator;
    public string? Creator
    {
        get => _creator;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _creator = null;
                CreatedById = null;
            }
            else if (ulong.TryParse(value, out var createdById))
            {
                CreatedById = createdById;
            }
            else
            {
                _creator = value;
            }
        }
    }

    public ulong? CreatedById { get; private set; }

    public bool ShowDeleted { get; set; }
}
