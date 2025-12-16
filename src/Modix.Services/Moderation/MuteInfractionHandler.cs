#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Serilog;

namespace Modix.Services.Moderation;

/// <summary>
/// Handles Mute infraction type behavior.
/// </summary>
public class MuteInfractionHandler(
    IUserService userService,
    IDesignatedRoleMappingRepository designatedRoleMappingRepository) : IInfractionTypeHandler
{
    public InfractionType Type => InfractionType.Mute;

    public AuthorizationClaim RequiredClaim => AuthorizationClaim.ModerationMute;

    public bool RequiresReason => false;

    public bool CanBeRescinded => true;

    public bool RequiresUniqueActiveInfraction => true;

    public bool RequiresRankValidation => true;

    public async Task ApplyInfractionAsync(IGuild guild, IGuildUser? subject, ulong subjectId, string reason)
    {
        if (subject is not null)
        {
            await subject.AddRoleAsync(await GetDesignatedMuteRoleAsync(guild));
        }
    }

    public async Task RescindInfractionAsync(IGuild guild, ulong subjectId, string? reason, InfractionSummary? infraction)
    {
        RequestOptions? GetRequestOptions() =>
            string.IsNullOrEmpty(reason) ? null : new RequestOptions { AuditLogReason = reason };

        if (!await userService.GuildUserExistsAsync(guild.Id, subjectId))
        {
            Log.Information(
                "Attempted to remove the mute role from {0} ({1}), but they were not in the server.",
                infraction?.Subject.Nickname ?? "Unknown user",
                subjectId);
            return;
        }

        var subject = await userService.GetGuildUserAsync(guild.Id, subjectId);
        await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild), GetRequestOptions());
    }

    public async Task DeleteInfractionAsync(IGuild guild, ulong subjectId, InfractionSummary infraction)
    {
        if (await userService.GuildUserExistsAsync(guild.Id, subjectId))
        {
            var subject = await userService.GetGuildUserAsync(guild.Id, subjectId);
            await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild));
        }
        else
        {
            Log.Warning(
                "Tried to unmute {User} while deleting mute infraction, but they weren't in the guild: {Guild}",
                subjectId, guild.Id);
        }
    }

    private async Task<IRole> GetDesignatedMuteRoleAsync(IGuild guild)
    {
        var mapping = (await designatedRoleMappingRepository.SearchBriefsAsync(
            new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guild.Id,
                Type = DesignatedRoleType.ModerationMute,
                IsDeleted = false
            })).FirstOrDefault();

        if (mapping == null)
            throw new InvalidOperationException(
                $"There are currently no designated mute roles within guild {guild.Id}");

        return guild.Roles.First(x => x.Id == mapping.Role.Id);
    }
}
