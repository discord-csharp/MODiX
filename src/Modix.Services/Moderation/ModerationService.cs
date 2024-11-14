#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Serilog;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Services.MessageContentPatterns;

namespace Modix.Services.Moderation;

public class ModerationService(
    IDiscordClient discordClient,
    IAuthorizationService authorizationService,
    IChannelService channelService,
    IUserService userService,
    IModerationActionRepository moderationActionRepository,
    IInfractionRepository infractionRepository,
    IDeletedMessageRepository deletedMessageRepository,
    IDeletedMessageBatchRepository deletedMessageBatchRepository,
    IScopedSession scopedSession,
    ModixContext db)
{
    public const string MUTE_ROLE_NAME = "MODiX_Moderation_Mute";
    private const int MAX_REASON_LENGTH = 1000;

    public async Task AutoRescindExpiredInfractions()
    {
        var expiredInfractions = await db
            .Set<InfractionEntity>()
            .Where(x => x.RescindActionId == null)
            .Where(x => x.DeleteActionId == null)
            .Where(x => x.CreateAction.Created + x.Duration <= DateTime.UtcNow)
            .Select(x => new { x.Id, x.GuildId, x.SubjectId }).ToListAsync();

        foreach (var expiredInfraction in expiredInfractions)
        {
            await RescindInfraction(expiredInfraction.Id, "Expired");
        }
    }

    public async Task<ServiceResponse> AddInfraction(ulong guildId,
        ulong addedByUserId,
        InfractionType type,
        ulong subjectId,
        string reason,
        TimeSpan? duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reason.Length >= MAX_REASON_LENGTH)
        {
            return ServiceResponse.Fail($"Reason is too long, must be less than {MAX_REASON_LENGTH} characters");
        }

        var hasClaim = await scopedSession.HasClaim(_createInfractionClaimsByType[type]);

        if (!hasClaim)
        {
            return ServiceResponse.Fail("Missing claim for infraction type");
        }

        var guild = await discordClient.GetGuildAsync(guildId);
        var subject = await userService.TryGetGuildUserAsync(guild, subjectId, default);
        await RequireSubjectRankLowerThanModeratorRank(guild, addedByUserId, subject);

        if (type is InfractionType.Mute or InfractionType.Ban)
        {
            var hasExistingInfractionForType = await db.Set<InfractionEntity>()
                .Where(x => x.GuildId == guildId)
                .Where(x => x.SubjectId == subjectId)
                .Where(x => x.Type == type)
                .Where(x => x.DeleteActionId == null)
                .Where(x => x.RescindActionId == null)
                .AnyAsync();

            if (hasExistingInfractionForType)
            {
                return ServiceResponse.Fail("This infraction already exists");
            }
        }

        var infraction = new InfractionEntity
        {
            GuildId = guildId,
            Type = type,
            SubjectId = subjectId,
            Reason = reason,
            Duration = duration,
            CreateAction = new ModerationActionEntity
            {
                GuildId = guildId,
                Type = ModerationActionType.InfractionCreated,
                CreatedById = addedByUserId,
                Created = DateTime.UtcNow,
            }
        };

        db.Add(infraction);

        await db.SaveChangesAsync();

        switch (type)
        {
            case InfractionType.Mute when subject is not null:
                await subject.AddRoleAsync(
                    await GetDesignatedMuteRole(guild));
                break;

            case InfractionType.Ban:
                await guild.AddBanAsync(subjectId, reason: reason);
                break;
        }

        return ServiceResponse.Ok();
    }

    public async Task RescindInfraction(InfractionType type, ulong guildId, ulong subjectId, string? reason = null)
    {
        var targetInfraction = await db.Set<InfractionEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.SubjectId == subjectId)
            .Where(x => x.Type == type)
            .Where(x => x.RescindActionId == null)
            .Where(x => x.DeleteActionId == null)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync();

        if (targetInfraction is null)
            return;

        await RescindInfraction(targetInfraction.Value, reason);
    }

    public async Task RescindInfractionAsync(InfractionType type, ulong guildId, ulong subjectId,
        string? reason = null)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationRescind);

        if (!hasClaim)
            return;

        var infraction = await db.Set<InfractionEntity>()
            .Where(x => x.Id == infractionId)
            .Select(x =>
                new
                {
                    Entity = x,
                    x.GuildId,
                    x.SubjectId,
                    x.Type,
                    x.Subject.User.Username
                }).SingleAsync();

        await RequireSubjectRankLowerThanModeratorRank(infraction.GuildId,
            scopedSession.ExecutingUserId,
            infraction.GuildId);

        RequestOptions? GetRequestOptions() =>
            string.IsNullOrEmpty(reason) ? null : new RequestOptions { AuditLogReason = reason };

        var guild = await discordClient.GetGuildAsync(infraction.GuildId);

        switch (infraction.Type)
        {
            case InfractionType.Mute:

                if (!await userService.GuildUserExistsAsync(infraction.GuildId, infraction.SubjectId))
                {
                    Log.Information(
                        "Attempted to remove the mute role from {Username} ({SubjectId}), but they were not in the server",
                        infraction.Username,
                        infraction.SubjectId);
                    break;
                }

                var subject = await userService.GetGuildUserAsync(infraction.GuildId, infraction.SubjectId);
                await subject.RemoveRoleAsync(await GetDesignatedMuteRole(guild), GetRequestOptions());
                break;

            case InfractionType.Ban:
                await guild.RemoveBanAsync(infraction.SubjectId, GetRequestOptions());
                break;

            default:
                throw new InvalidOperationException($"{infraction.Type} infractions cannot be rescinded");
        }

        var entity = new ModerationActionEntity()
        {
            GuildId = infraction.GuildId,
            Type = ModerationActionType.InfractionRescinded,
            Created = DateTimeOffset.UtcNow,
            CreatedById = scopedSession.ExecutingUserId,
            InfractionId = infractionId
        };

        infraction.Entity.RescindReason = reason;

        db.Add(entity);

        await db.SaveChangesAsync();

        await infractionRepository.TryRescindAsync(infractionid, authorizationService.CurrentUserId!.Value, reason);
    }

    public async Task DeleteInfractionAsync(long infractionId)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationDeleteInfraction);

        if (!hasClaim)
            return;

        var infraction = await db
            .Set<InfractionEntity>()
            .Where(x => x.Id == infractionId)
            .Select(x =>
                new
                {
                    x.GuildId,
                    x.SubjectId,
                    x.Type,
                    x.Subject.User.Username,
                    x.RescindActionId
                }).SingleAsync();

        await RequireSubjectRankLowerThanModeratorRank(infraction.GuildId,
            scopedSession.ExecutingUserId,
            infraction.SubjectId);

        var guild = await discordClient.GetGuildAsync(infraction.GuildId);

        switch (infraction.Type)
        {
            case InfractionType.Mute:

                if (await userService.GuildUserExistsAsync(infraction.GuildId, infraction.SubjectId))
                {
                    var subject = await userService.GetGuildUserAsync(infraction.GuildId, infraction.SubjectId);
                    await subject.RemoveRoleAsync(await GetDesignatedMuteRole(guild));
                }
                else
                {
                    Log.Warning(
                        "Tried to unmute {User} while deleting mute infraction, but they weren't in the guild: {Guild}",
                        infraction.SubjectId,
                        infraction.GuildId);
                }

                break;

            case InfractionType.Ban:

                // If the infraction has already been rescinded, we don't need to actually perform the unmute/unban
                // Doing so will return a 404 from Discord (trying to remove a non-existent ban)
                if (infraction.RescindActionId is null)
                {
                    await guild.RemoveBanAsync(infraction.SubjectId);
                }

                break;
        }

        var entity = new ModerationActionEntity()
        {
            GuildId = infraction.GuildId,
            Type = ModerationActionType.InfractionDeleted,
            Created = DateTimeOffset.UtcNow,
            CreatedById = scopedSession.ExecutingUserId,
            InfractionId = infractionId
        };

        db.Add(entity);
        await db.SaveChangesAsync();

        await infractionRepository.TryDeleteAsync(infraction.Id, authorizationService.CurrentUserId.Value);
    }

    public async Task DeleteMessageAsync(IMessage message, string reason, ulong deletedById,
        CancellationToken cancellationToken)
    {
        if (message.Channel is not IGuildChannel guildChannel)
            throw new InvalidOperationException(
                $"Cannot delete message {message.Id} because it is not a guild message");

        await userService.TrackUserAsync((IGuildUser)message.Author, cancellationToken);
        await channelService.TrackChannelAsync(guildChannel.Name, guildChannel.Id, guildChannel.GuildId,
            guildChannel is IThreadChannel threadChannel ? threadChannel.CategoryId : null, cancellationToken);

        using var transaction = await deletedMessageRepository.BeginCreateTransactionAsync(cancellationToken);

        await deletedMessageRepository.CreateAsync(
            new DeletedMessageCreationData()
            {
                GuildId = guildChannel.GuildId,
                ChannelId = guildChannel.Id,
                MessageId = message.Id,
                AuthorId = message.Author.Id,
                Content = message.Content,
                Reason = reason,
                CreatedById = deletedById
            }, cancellationToken);

        await message.DeleteAsync(new RequestOptions() { CancelToken = cancellationToken });

        transaction.Commit();
    }

    public async Task DeleteMessagesAsync(ITextChannel channel, int count, bool skipOne,
        Func<Task<bool>> confirmDelegate)
    {
        authorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

        if (confirmDelegate is null)
            throw new ArgumentNullException(nameof(confirmDelegate));

        if (!(channel is IGuildChannel guildChannel))
            throw new InvalidOperationException(
                $"Cannot delete messages in {channel.Name} because it is not a guild channel.");

        var confirmed = await confirmDelegate();

        if (!confirmed)
            return;

        var clampedCount = Math.Clamp(count, 0, 100);

        if (clampedCount == 0)
            return;

        var messages = skipOne
            ? (await channel.GetMessagesAsync(clampedCount + 1).FlattenAsync()).Skip(1)
            : await channel.GetMessagesAsync(clampedCount).FlattenAsync();

        await DoDeleteMessagesAsync(channel, guildChannel, messages);
    }

    public async Task DeleteMessagesAsync(ITextChannel channel, IGuildUser user, int count,
        Func<Task<bool>> confirmDelegate)
    {
        authorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

        if (confirmDelegate is null)
            throw new ArgumentNullException(nameof(confirmDelegate));

        if (!(channel is IGuildChannel guildChannel))
            throw new InvalidOperationException(
                $"Cannot delete messages in {channel.Name} because it is not a guild channel.");

        var confirmed = await confirmDelegate();

        if (!confirmed)
            return;

        var clampedCount = Math.Clamp(count, 0, 100);

        if (clampedCount == 0)
            return;

        var messages = (await channel.GetMessagesAsync(100).FlattenAsync()).Where(x => x.Author.Id == user.Id)
            .Take(clampedCount);

        await DoDeleteMessagesAsync(channel, guildChannel, messages);
    }

    public async Task<RecordsPage<DeletedMessageSummary>> SearchDeletedMessagesAsync(
        DeletedMessageSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria,
        PagingCriteria pagingCriteria)
    {
        authorizationService.RequireClaims(AuthorizationClaim.LogViewDeletedMessages);

        return await deletedMessageRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria,
            pagingCriteria);
    }

    public Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(
        InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null)
    {
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

        return infractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);
    }

    public Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria,
        IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
    {
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

        return infractionRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);
    }

    public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId)
    {
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

        return await infractionRepository.GetInfractionCountsAsync(new InfractionSearchCriteria
        {
            GuildId = authorizationService.CurrentGuildId, SubjectId = subjectId, IsDeleted = false
        });
    }

    public Task<ModerationActionSummary?> GetModerationActionSummaryAsync(long moderationActionId)
    {
        return moderationActionRepository.ReadSummaryAsync(moderationActionId);
    }

    public Task<DateTimeOffset?> GetNextInfractionExpiration()
        => infractionRepository.ReadExpiresFirstOrDefaultAsync(
            new InfractionSearchCriteria()
            {
                IsRescinded = false,
                IsDeleted = false,
                ExpiresRange = new DateTimeOffsetRange()
                {
                    From = DateTimeOffset.MinValue, To = DateTimeOffset.MaxValue,
                }
            },
            new[]
            {
                new SortingCriteria()
                {
                    PropertyName = nameof(InfractionSummary.Expires), Direction = SortDirection.Ascending
                }
            });

    public async Task<bool> DoesModeratorOutrankUser(ulong guildId, ulong moderatorId, ulong subjectId)
    {
        //If the user doesn't exist in the guild, we outrank them
        if (await userService.GuildUserExistsAsync(guildId, subjectId) == false)
            return true;

        var subject = await userService.GetGuildUserAsync(guildId, subjectId);

        return await DoesModeratorOutrankUser(subject.Guild, moderatorId, subject);
    }

    public async Task<bool> AnyActiveInfractions(ulong guildId, ulong userId, InfractionType? type = null)
    {
        return await db
            .Set<InfractionEntity>()
            .Where(x => x.GuildId == guildId)
            .Where(x => x.SubjectId == userId)
            .Where(x => x.DeleteActionId == null)
            .Where(x => x.RescindActionId == null)
            .Where(x => type == null || x.Type == type)
            .AnyAsync();
    }

    public async Task<(bool success, string? errorMessage)> UpdateInfractionAsync(long infractionId,
        string newReason, ulong currentUserId)
    {
        var infraction = await infractionRepository.ReadSummaryAsync(infractionId);

        if (infraction is null)
            return (false, $"An infraction with an ID of {infractionId} could not be found.");

        authorizationService.RequireClaims(_createInfractionClaimsByType[infraction.Type]);

        // Allow users who created the infraction to bypass any further
        // validation and update their own infraction
        if (infraction.CreateAction.CreatedBy.Id == currentUserId)
        {
            return (await infractionRepository.TryUpdateAsync(infractionId, newReason, currentUserId), null);
        }

        // Else we know it's not the user's infraction
        authorizationService.RequireClaims(AuthorizationClaim.ModerationUpdateInfraction);

        return (await infractionRepository.TryUpdateAsync(infractionId, newReason, currentUserId), null);
    }

    private async Task DoDeleteMessagesAsync(ITextChannel channel, IGuildChannel guildChannel,
        IEnumerable<IMessage> messages)
    {
        await channel.DeleteMessagesAsync(messages);

        using var transaction = await deletedMessageBatchRepository.BeginCreateTransactionAsync();
        await channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId,
            channel is IThreadChannel threadChannel ? threadChannel.CategoryId : null);

        await deletedMessageBatchRepository.CreateAsync(new DeletedMessageBatchCreationData()
        {
            CreatedById = authorizationService.CurrentUserId!.Value,
            GuildId = authorizationService.CurrentGuildId!.Value,
            Data = messages.Select(
                x => new DeletedMessageCreationData()
                {
                    AuthorId = x.Author.Id,
                    ChannelId = x.Channel.Id,
                    Content = x.Content,
                    GuildId = authorizationService.CurrentGuildId.Value,
                    MessageId = x.Id,
                    Reason = "Mass-deleted.",
                }),
        });

        transaction.Commit();
    }

    private async Task<IRole> GetDesignatedMuteRole(IGuild guild)
    {
        var muteDesignationId = await db.Set<DesignatedRoleMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.Type == DesignatedRoleType.ModerationMute)
            .Where(x => x.DeleteActionId == null)
            .Select(x => x.RoleId)
            .SingleAsync();

        return guild.Roles.First(x => x.Id == muteDesignationId);
    }

    private async Task RequireSubjectRankLowerThanModeratorRank(ulong guildId, ulong moderatorId,
        ulong subjectId)
    {
        if (!await DoesModeratorOutrankUser(guildId, moderatorId, subjectId))
            throw new InvalidOperationException(
                "Cannot moderate users that have a rank greater than or equal to your own.");
    }

    private async ValueTask RequireSubjectRankLowerThanModeratorRank(IGuild guild, ulong moderatorId,
        IGuildUser? subject)
    {
        // If the subject is null, then the moderator automatically outranks them.
        if (subject is null)
            return;

        if (!await DoesModeratorOutrankUser(guild, moderatorId, subject))
            throw new InvalidOperationException(
                "Cannot moderate users that have a rank greater than or equal to your own.");
    }

    private async Task<bool> DoesModeratorOutrankUser(IGuild guild, ulong moderatorId, IGuildUser subject)
    {
        //If the subject is the guild owner, and the moderator is not the owner, the moderator does not outrank them
        if (guild.OwnerId == subject.Id && guild.OwnerId != moderatorId)
            return false;

        var moderator = await userService.GetGuildUserAsync(guild.Id, moderatorId);

        // If the moderator has the "Admin" permission, they outrank everyone in the guild but the owner
        if (moderator.GuildPermissions.Administrator)
            return true;

        var rankRoles = await db
            .Set<DesignatedRoleMappingEntity>()
            .Where(x => x.GuildId == guild.Id)
            .Where(x => x.Type == DesignatedRoleType.Rank)
            .Where(x => x.DeleteActionId == null)
            .Select(x => new
            {
                Id = x.RoleId,
                x.Role.Position
            })
            .ToListAsync();

        var subjectRankRoles = rankRoles.Where(r => subject.RoleIds.Contains(r.Id));
        var moderatorRankRoles = rankRoles.Where(r => moderator.RoleIds.Contains(r.Id));

        var greatestSubjectRankPosition = subjectRankRoles.Any()
            ? subjectRankRoles.Select(r => r.Position).Max()
            : int.MinValue;
        var greatestModeratorRankPosition = moderatorRankRoles.Any()
            ? moderatorRankRoles.Select(r => r.Position).Max()
            : int.MinValue;

        return greatestSubjectRankPosition < greatestModeratorRankPosition;
    }

    private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
        = new()
        {
            { InfractionType.Notice, AuthorizationClaim.ModerationNote },
            { InfractionType.Warning, AuthorizationClaim.ModerationWarn },
            { InfractionType.Mute, AuthorizationClaim.ModerationMute },
            { InfractionType.Ban, AuthorizationClaim.ModerationBan }
        };
}
