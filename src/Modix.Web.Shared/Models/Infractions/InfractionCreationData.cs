using Modix.Models.Moderation;

namespace Modix.Web.Shared.Models.Infractions;

public record InfractionCreationData(InfractionType Type, ulong SubjectId, string Reason, TimeSpan? Duration);
