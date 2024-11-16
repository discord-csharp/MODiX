using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services;

public class DesignatedChannelService(
    ModixContext db,
    IAuthorizationService authorizationService)
{
    public async Task<long> AddDesignatedChannel(IGuild guild, IMessageChannel logChannel,
        DesignatedChannelType type)
    {
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingCreate);

        var designationId = await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.ChannelId == logChannel.Id)
            .Where(x => x.Type == type)
            .Where(x => x.DeleteActionId == null)
            .Select(x => (long?)x.Id)
            .SingleOrDefaultAsync();

        if (designationId is not null)
        {
            return designationId.Value;
        }

        var newEntity = new DesignatedChannelMappingEntity
        {
            GuildId = guild.Id,
            ChannelId = logChannel.Id,
            Type = type,
            CreateAction = new ConfigurationActionEntity
            {
                GuildId = guild.Id,
                Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                Created = DateTimeOffset.UtcNow,
                CreatedById = authorizationService.CurrentUserId!.Value,
            }
        };

        db.Add(newEntity);
        await db.SaveChangesAsync();

        return newEntity.Id;
    }

    public async Task RemoveDesignatedChannel(IGuild guild, IMessageChannel logChannel, DesignatedChannelType type)
    {
        var designationId = await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.ChannelId == logChannel.Id)
            .Where(x => x.Type == type)
            .Where(x => x.DeleteActionId == null)
            .Select(x => (long?)x.Id)
            .SingleOrDefaultAsync();

        if (designationId is null)
        {
            return;
        }

        await RemoveDesignatedChannelById(designationId.Value);
    }

    public async Task RemoveDesignatedChannelById(long designationId)
    {
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingDelete);

        var designation = await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.Id == designationId)
            .SingleAsync();

        var deleteAction = new ConfigurationActionEntity
        {
            Type = ConfigurationActionType.DesignatedChannelMappingDeleted,
            Created = DateTimeOffset.UtcNow,
            CreatedById = authorizationService.CurrentUserId!.Value,
            DesignatedChannelMappingId = designation.Id,
            GuildId = designation.GuildId,
        };

        db.Add(deleteAction);
        await db.SaveChangesAsync();
    }

    public async Task<bool> HasDesignatedChannelForType(ulong guildId, DesignatedChannelType type)
    {
        return await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.Type == type)
            .Where(x => x.DeleteActionId == null)
            .AnyAsync();
    }

    public async Task<IReadOnlyCollection<ulong>> GetDesignatedChannelIds(ulong guildId, DesignatedChannelType type)
    {
        return await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.Type == type)
            .Where(x => x.DeleteActionId == null)
            .Select(x => x.ChannelId)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IMessageChannel>> GetDesignatedChannels(IGuild guild,
        DesignatedChannelType type)
    {
        var channelIds = await GetDesignatedChannelIds(guild.Id, type);

        if (!channelIds.Any())
        {
            return [];
        }

        var channels = await Task.WhenAll(channelIds.Select(d => guild.GetChannelAsync(d)));

        return channels
            .OfType<IMessageChannel>()
            .ToArray();
    }

    public async Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> GetDesignatedChannels(ulong guildId)
    {
        authorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingRead);

        return await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.DeleteActionId == null)
            .Select(x => new DesignatedChannelMappingBrief
            {
                Id = x.Id,
                Channel = new GuildChannelBrief
                {
                    Id = x.ChannelId,
                    Name = x.Channel.Name,
                    ParentChannelId = x.Channel.ParentChannelId,
                },
                Type = x.Type,
            }).ToListAsync();
    }

    public async Task<bool> ChannelHasDesignation(
        ulong guildId,
        ulong channelId,
        DesignatedChannelType type,
        CancellationToken cancellationToken)
    {
        return await db
            .Set<DesignatedChannelMappingEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.ChannelId == channelId)
            .Where(x => x.Type == type)
            .Where(x => x.DeleteActionId == null)
            .AnyAsync(cancellationToken: cancellationToken);
    }
}
