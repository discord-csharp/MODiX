#nullable enable
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation;

/// <summary>
/// Defines the behavior for a specific infraction type.
/// </summary>
public interface IInfractionTypeHandler
{
    /// <summary>
    /// The infraction type that this handler manages.
    /// </summary>
    InfractionType Type { get; }

    /// <summary>
    /// The authorization claim required to create this infraction type.
    /// </summary>
    AuthorizationClaim RequiredClaim { get; }

    /// <summary>
    /// Whether this infraction type requires a non-empty reason.
    /// </summary>
    bool RequiresReason { get; }

    /// <summary>
    /// Whether this infraction type can be rescinded.
    /// </summary>
    bool CanBeRescinded { get; }

    /// <summary>
    /// Whether this infraction type requires checking for existing active infractions.
    /// </summary>
    bool RequiresUniqueActiveInfraction { get; }

    /// <summary>
    /// Whether this infraction type requires rank validation (moderator must outrank subject).
    /// </summary>
    bool RequiresRankValidation { get; }

    /// <summary>
    /// Applies the infraction to Discord (e.g., adds mute role, bans user).
    /// </summary>
    Task ApplyInfractionAsync(IGuild guild, IGuildUser? subject, ulong subjectId, string reason);

    /// <summary>
    /// Rescinds the infraction from Discord (e.g., removes mute role, unbans user).
    /// </summary>
    Task RescindInfractionAsync(IGuild guild, ulong subjectId, string? reason, InfractionSummary? infraction);

    /// <summary>
    /// Deletes the infraction from Discord (e.g., removes mute role, unbans user if not already rescinded).
    /// </summary>
    Task DeleteInfractionAsync(IGuild guild, ulong subjectId, InfractionSummary infraction);
}
