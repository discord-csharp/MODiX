#nullable enable
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation;

/// <summary>
/// Handles Warning infraction type behavior.
/// </summary>
public class WarningInfractionHandler : IInfractionTypeHandler
{
    public InfractionType Type => InfractionType.Warning;

    public AuthorizationClaim RequiredClaim => AuthorizationClaim.ModerationWarn;

    public bool RequiresReason => true;

    public bool CanBeRescinded => false;

    public bool RequiresUniqueActiveInfraction => false;

    public bool RequiresRankValidation => false;

    public Task ApplyInfractionAsync(IGuild guild, IGuildUser? subject, ulong subjectId, string reason)
    {
        // Warnings don't require any Discord action
        return Task.CompletedTask;
    }

    public Task RescindInfractionAsync(IGuild guild, ulong subjectId, string? reason, InfractionSummary? infraction)
    {
        // Warnings cannot be rescinded
        return Task.CompletedTask;
    }

    public Task DeleteInfractionAsync(IGuild guild, ulong subjectId, InfractionSummary infraction)
    {
        // Warnings don't require any Discord action when deleted
        return Task.CompletedTask;
    }
}
