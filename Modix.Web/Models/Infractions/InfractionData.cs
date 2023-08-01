using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Web.Models.Infractions;

public record InfractionData(
    long Id,
    ulong GuildId,
    InfractionType Type,
    string Reason,
    TimeSpan? Duration,
    GuildUserBrief Subject,
    ModerationActionBrief CreateAction,
    ModerationActionBrief? RescindAction,
    ModerationActionBrief? DeleteAction,
    bool CanBeRescind,
    bool CanBeDeleted
)
{
    public static InfractionData FromInfractionSummary(InfractionSummary summary, Dictionary<ulong, bool> outranksValues)
    {
        return new InfractionData(
                    summary.Id,
                    summary.GuildId,
                    summary.Type,
                    summary.Reason,
                    summary.Duration,
                    summary.Subject,

                    summary.CreateAction,
                    summary.RescindAction,
                    summary.DeleteAction,

                    summary.RescindAction is null
                            && summary.DeleteAction is null
                            && (summary.Type == InfractionType.Mute || summary.Type == InfractionType.Ban)
                            && outranksValues[summary.Subject.Id],

                    summary.DeleteAction is null
                        && outranksValues[summary.Subject.Id]
                    );
    }
}
