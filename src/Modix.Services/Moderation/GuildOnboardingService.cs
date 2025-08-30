using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Serilog;

namespace Modix.Services.Moderation;

public class GuildOnboardingService(
    ModixContext db,
    IScopedSession scopedSession,
    IRoleService roleService,
    IUserService userService,
    DesignatedChannelService designatedChannelService,
    AuthorizationClaimMappingService authorizationClaimService)
{
    private static readonly OverwritePermissions _mutePermissions
        = new(
            addReactions: PermValue.Deny,
            requestToSpeak: PermValue.Deny,
            sendMessages: PermValue.Deny,
            sendMessagesInThreads: PermValue.Deny,
            speak: PermValue.Deny,
            usePrivateThreads: PermValue.Deny,
            usePublicThreads: PermValue.Deny);

    public async Task AutoConfigureGuild(IGuild guild)
    {
        await userService.TrackUserAsync(guild, scopedSession.ExecutingUserId);
        await EnsureMuteRoleIsConfigured(guild);
        await EnsureClaimsAreConfigured(guild);
    }

    public async Task AutoConfigureChannel(IChannel channel)
    {
        if (channel is IGuildChannel guildChannel)
        {
            var isUnmoderated = await designatedChannelService.ChannelHasDesignation(guildChannel.Guild.Id,
                channel.Id, DesignatedChannelType.Unmoderated, default);

            if (isUnmoderated)
            {
                return;
            }

            var muteRole = await EnsureMuteRuleExists(guildChannel.Guild);
            await ConfigureChannelMuteRolePermissions(guildChannel, muteRole);
        }
    }

    private async Task EnsureClaimsAreConfigured(IGuild guild)
    {
        var hasClaimsInGuild = await db
            .Set<ClaimMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.DeleteActionId == null)
            .AnyAsync();

        if (hasClaimsInGuild)
            return; // Already configured, probably

        foreach (var claim in Enum.GetValues<AuthorizationClaim>())
        foreach (var role in guild.Roles.Where(x => x.Permissions.Administrator))
        {
            await authorizationClaimService.AddClaimForRole(guild.Id, role.Id, claim);
        }
    }

    private async Task EnsureMuteRoleIsConfigured(IGuild guild)
    {
        var muteRole = await EnsureMuteRuleExists(guild);

        var channelsInGuild = await guild.GetChannelsAsync();

        var unmoderatedChannels = await designatedChannelService.GetDesignatedChannelIds(guild.Id,
            DesignatedChannelType.Unmoderated);

        var nonCategoryChannels =
            channelsInGuild
            .Where(c => c is (ITextChannel or IVoiceChannel) and not IThreadChannel)
            .Where(c => !unmoderatedChannels.Contains(c.Id))
            .ToList();

        var setUpChannels = new List<IGuildChannel>();

        try
        {
            foreach (var channel in nonCategoryChannels)
            {
                setUpChannels.Add(channel);
                await ConfigureChannelMuteRolePermissions(channel, muteRole);
            }
        }
        catch (HttpException ex)
        {
            var errorTemplate =
                "An exception was thrown when attempting to set up the mute role {Role} for guild {Guild}, channel #{Channel}. " +
                "This is likely due to Modix not having the \"Manage Permissions\" permission - please check your server settings.";

            Log.Error(ex, errorTemplate, muteRole.Name, guild.Name, setUpChannels.Last().Name);

            return;
        }

        Log.Information("Successfully configured mute role @{MuteRole} for {ChannelCount} channels: {Channels}",
            muteRole.Name, nonCategoryChannels.Count, nonCategoryChannels.Select(c => c.Name));
    }

    private async Task<IRole> EnsureMuteRuleExists(IGuild guild)
    {
        var hasRoleMapping = await db.Set<DesignatedRoleMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.Type == DesignatedRoleType.ModerationMute)
            .Where(x => x.DeleteActionId == null)
            .AnyAsync();

        var role = guild.Roles.FirstOrDefault(x => x.Name == ModerationService.MUTE_ROLE_NAME)
                   ?? await guild.CreateRoleAsync(ModerationService.MUTE_ROLE_NAME, isMentionable: false);

        if (!hasRoleMapping)
        {
            var entity = new DesignatedRoleMappingEntity
            {
                GuildId = guild.Id,
                RoleId = role.Id,
                Type = DesignatedRoleType.ModerationMute,
                CreateAction = new ConfigurationActionEntity
                {
                    GuildId = guild.Id,
                    Type = ConfigurationActionType.DesignatedRoleMappingCreated,
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = scopedSession.ExecutingUserId,
                }
            };

            db.Add(entity);
            await db.SaveChangesAsync();
        }

        await roleService.TrackRoleAsync(role, default);

        return role;
    }

    private static async Task ConfigureChannelMuteRolePermissions(IGuildChannel channel, IRole muteRole)
    {
        try
        {
            var permissionOverwrite = channel.GetPermissionOverwrite(muteRole);

            if (permissionOverwrite is null || _mutePermissions.ToDenyList().Any(x => !permissionOverwrite.GetValueOrDefault().ToDenyList().Contains(x)))
            {
                await channel.AddPermissionOverwriteAsync(muteRole, _mutePermissions, new() { AuditLogReason = "Setting mute role permissions." });
                Log.Debug("Set mute permissions for role {Role} in channel #{Channel}", muteRole.Name, channel.Name);
            }
            else
            {
                Log.Debug("Skipping setting mute permissions for channel #{Channel} as they're already set", channel.Name);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed setting channel mute role on #{Channel}", channel.Name);
            throw;
        }
    }
}
