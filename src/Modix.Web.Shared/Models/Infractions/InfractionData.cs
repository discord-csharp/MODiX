using Modix.Models.Moderation;

namespace Modix.Web.Shared.Models.Infractions;

public record InfractionData(
    long Id,
    ulong GuildId,
    InfractionType Type,
    string Reason,
    TimeSpan? Duration,
    string SubjectName,
    string CreatedBy,
    DateTimeOffset Created,
    bool IsRescinded,
    bool IsDeleted,
    bool CanBeRescinded,
    bool CanBeDeleted
);
