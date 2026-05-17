#nullable enable
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation;

/// <summary>
/// Handles Ban infraction type behavior.
/// </summary>
public class BanInfractionHandler : IInfractionTypeHandler
{
    public InfractionType Type => InfractionType.Ban;

    public AuthorizationClaim RequiredClaim => AuthorizationClaim.ModerationBan;

    public bool RequiresReason => false;

    public bool CanBeRescinded => true;

    public bool RequiresUniqueActiveInfraction => true;

    public bool RequiresRankValidation => true;

    public async Task ApplyInfractionAsync(IGuild guild, IGuildUser? subject, ulong subjectId, string reason)
    {
        await guild.AddBanAsync(subjectId, reason: reason);
    }

    public async Task RescindInfractionAsync(IGuild guild, ulong subjectId, string? reason, InfractionSummary? infraction)
    {
        RequestOptions? GetRequestOptions() =>
            string.IsNullOrEmpty(reason) ? null : new RequestOptions { AuditLogReason = reason };

        await guild.RemoveBanAsync(subjectId, GetRequestOptions());
    }

    public async Task DeleteInfractionAsync(IGuild guild, ulong subjectId, InfractionSummary infraction)
    {
        // If the infraction has already been rescinded, we don't need to actually perform the unban
        // Doing so will return a 404 from Discord (trying to remove a nonexistent ban)
        if (infraction.RescindAction is null)
        {
            await guild.RemoveBanAsync(subjectId);
        }
    }
}
