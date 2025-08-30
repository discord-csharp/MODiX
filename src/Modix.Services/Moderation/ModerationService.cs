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
using Modix.Services.Utilities;
using Serilog;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Modix.Data;

namespace Modix.Services.Moderation;

public class ModerationService(
    IDiscordClient discordClient,
    IAuthorizationService authorizationService,
    IChannelService channelService,
    IUserService userService,
    IModerationActionRepository moderationActionRepository,
    IDesignatedRoleMappingRepository designatedRoleMappingRepository,
    IInfractionRepository infractionRepository,
    IDeletedMessageRepository deletedMessageRepository,
    IDeletedMessageBatchRepository deletedMessageBatchRepository,
    ModixContext db)
{
    public const string MUTE_ROLE_NAME = "MODiX_Moderation_Mute";
    private const int MaxReasonLength = 1000;

    public async Task AutoRescindExpiredInfractions()
    {
        var expiredInfractions = await db
            .Set<InfractionEntity>()
            .Where(x => x.RescindActionId == null)
            .Where(x => x.DeleteActionId == null)
            .Where(x => x.CreateAction.Created + x.Duration <= DateTime.UtcNow)
            .Select(x => new
            {
                x.Id,
                x.GuildId,
                x.SubjectId
            }).ToListAsync();

        foreach (var expiredInfraction in expiredInfractions)
        {
            await RescindInfractionAsync(expiredInfraction.Id,
                expiredInfraction.GuildId,
                expiredInfraction.SubjectId,
                isAutoRescind: true);
        }
    }

    public async Task CreateInfractionAsync(ulong guildId, ulong moderatorId, InfractionType type, ulong subjectId,
        string reason, TimeSpan? duration)
    {
        authorizationService.RequireClaims(_createInfractionClaimsByType[type]);

        if (reason is null)
            throw new ArgumentNullException(nameof(reason));

        if (reason.Length >= MaxReasonLength)
            throw new ArgumentException($"Reason must be less than {MaxReasonLength} characters in length",
                nameof(reason));

        if (type is InfractionType.Notice or InfractionType.Warning && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException($"{type.ToString()} infractions require a reason to be given");

        var guild = await discordClient.GetGuildAsync(guildId);
        var subject = await userService.TryGetGuildUserAsync(guild, subjectId, CancellationToken.None);

        using (var transaction = await infractionRepository.BeginCreateTransactionAsync())
        {
            if (type is InfractionType.Ban or InfractionType.Mute)
            {
                await RequireSubjectRankLowerThanModeratorRankAsync(guild, moderatorId, subject);

                if (await infractionRepository.AnyAsync(new InfractionSearchCriteria()
                    {
                        GuildId = guildId,
                        Types = [type],
                        SubjectId = subjectId,
                        IsRescinded = false,
                        IsDeleted = false
                    }))
                    throw new InvalidOperationException(
                        $"Discord user {subjectId} already has an active {type} infraction");
            }

            await infractionRepository.CreateAsync(
                new InfractionCreationData()
                {
                    GuildId = guildId,
                    Type = type,
                    SubjectId = subjectId,
                    Reason = reason,
                    Duration = duration,
                    CreatedById = moderatorId
                });

            transaction.Commit();
        }

        // TODO: Implement ModerationSyncBehavior to listen for mutes and bans that happen directly in Discord, instead of through bot commands,
        // and to read the Discord Audit Log to check for mutes and bans that were missed during downtime, and add all such actions to
        // the Infractions and ModerationActions repositories.
        // Note that we'll need to upgrade to the latest Discord.NET version to get access to the audit log.

        // Assuming that our Infractions repository is always correct, regarding the state of the Discord API.
        switch (type)
        {
            case InfractionType.Mute when subject is not null:
                await subject.AddRoleAsync(
                    await GetDesignatedMuteRoleAsync(guild));
                break;

            case InfractionType.Ban:
                await guild.AddBanAsync(subjectId, reason: reason);
                break;
        }
    }

    public async Task RescindInfractionAsync(long infractionId, string? reason = null)
    {
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

        var infraction = await infractionRepository.ReadSummaryAsync(infractionId);
        await DoRescindInfractionAsync(infraction!.Type, infraction.GuildId, infraction.Subject.Id, infraction,
            reason);
    }

    public async Task RescindInfractionAsync(InfractionType type, ulong guildId, ulong subjectId,
        string? reason = null)
    {
        authorizationService.RequireAuthenticatedGuild();
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

        if (reason?.Length >= MaxReasonLength)
            throw new ArgumentException($"Reason must be less than {MaxReasonLength} characters in length",
                nameof(reason));

        var infraction = (await infractionRepository.SearchSummariesAsync(
            new InfractionSearchCriteria()
            {
                GuildId = authorizationService.CurrentGuildId.Value,
                Types = new[] {type},
                SubjectId = subjectId,
                IsRescinded = false,
                IsDeleted = false,
            })).FirstOrDefault();

        await DoRescindInfractionAsync(type, guildId, subjectId, infraction, reason);
    }

    private async Task RescindInfractionAsync(long infractionId, ulong guildId, ulong subjectId,
        string? reason = null, bool isAutoRescind = false)
    {
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

        var infraction = await infractionRepository.ReadSummaryAsync(infractionId);

        await DoRescindInfractionAsync(infraction!.Type, guildId, subjectId, infraction, reason, isAutoRescind);
    }

    public async Task DeleteInfractionAsync(long infractionId)
    {
        authorizationService.RequireAuthenticatedUser();
        authorizationService.RequireClaims(AuthorizationClaim.ModerationDeleteInfraction);

        var infraction = await infractionRepository.ReadSummaryAsync(infractionId);

        if (infraction == null)
            throw new InvalidOperationException($"Infraction {infractionId} does not exist");

        await RequireSubjectRankLowerThanModeratorRankAsync(infraction.GuildId,
            authorizationService.CurrentUserId.Value, infraction.Subject.Id);

        await infractionRepository.TryDeleteAsync(infraction.Id, authorizationService.CurrentUserId.Value);

        var guild = await discordClient.GetGuildAsync(infraction.GuildId);

        switch (infraction.Type)
        {
            case InfractionType.Mute:

                if (await userService.GuildUserExistsAsync(guild.Id, infraction.Subject.Id))
                {
                    var subject = await userService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);
                    await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild));
                }
                else
                {
                    Log.Warning(
                        "Tried to unmute {User} while deleting mute infraction, but they weren't in the guild: {Guild}",
                        infraction.Subject.Id, guild.Id);
                }

                break;

            case InfractionType.Ban:

                //If the infraction has already been rescinded, we don't need to actually perform the unmute/unban
                //Doing so will return a 404 from Discord (trying to remove a nonexistant ban)
                if (infraction.RescindAction is null)
                {
                    await guild.RemoveBanAsync(infraction.Subject.Id);
                }

                break;
        }
    }

    public async Task DeleteMessageAsync(IMessage message, string reason, ulong deletedById,
        CancellationToken cancellationToken)
    {
        if (message.Channel is not IGuildChannel guildChannel)
            throw new InvalidOperationException(
                $"Cannot delete message {message.Id} because it is not a guild message");

        await userService.TrackUserAsync((IGuildUser)message.Author, cancellationToken);
        await channelService.TrackChannelAsync(guildChannel.Name, guildChannel.Id, guildChannel.GuildId, guildChannel is IThreadChannel threadChannel ? threadChannel.CategoryId : null, cancellationToken);

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

        await message.DeleteAsync(new RequestOptions() {CancelToken = cancellationToken});

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

    public async Task<bool> DoesModeratorOutrankUserAsync(ulong guildId, ulong moderatorId, ulong subjectId)
    {
        //If the user doesn't exist in the guild, we outrank them
        if (await userService.GuildUserExistsAsync(guildId, subjectId) == false)
            return true;

        var subject = await userService.GetGuildUserAsync(guildId, subjectId);

        return await DoesModeratorOutrankUserAsync(subject.Guild, moderatorId, subject);
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
        await channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId, channel is IThreadChannel threadChannel ? threadChannel.CategoryId : null);

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

    private async Task DoRescindInfractionAsync(InfractionType type,
        ulong guildId,
        ulong subjectId,
        InfractionSummary? infraction,
        string? reason = null,
        bool isAutoRescind = false)
    {
        RequestOptions? GetRequestOptions() =>
            string.IsNullOrEmpty(reason) ? null : new RequestOptions {AuditLogReason = reason};

        if (!isAutoRescind)
        {
            await RequireSubjectRankLowerThanModeratorRankAsync(guildId, authorizationService.CurrentUserId!.Value,
                subjectId);
        }

        var guild = await discordClient.GetGuildAsync(guildId);

        switch (type)
        {
            case InfractionType.Mute:
                if (!await userService.GuildUserExistsAsync(guild.Id, subjectId))
                {
                    Log.Information(
                        "Attempted to remove the mute role from {0} ({1}), but they were not in the server.",
                        infraction?.Subject.GetFullUsername() ?? "Unknown user",
                        subjectId);
                    break;
                }

                var subject = await userService.GetGuildUserAsync(guild.Id, subjectId);
                await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild), GetRequestOptions());
                break;

            case InfractionType.Ban:
                await guild.RemoveBanAsync(subjectId, GetRequestOptions());
                break;

            default:
                throw new InvalidOperationException($"{type} infractions cannot be rescinded.");
        }

        if (infraction != null)
        {
            await infractionRepository.TryRescindAsync(infraction.Id, authorizationService.CurrentUserId!.Value,
                reason);
        }
    }

    private async Task<IRole> GetDesignatedMuteRoleAsync(IGuild guild)
    {
        var mapping = (await designatedRoleMappingRepository.SearchBriefsAsync(
            new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guild.Id, Type = DesignatedRoleType.ModerationMute, IsDeleted = false
            })).FirstOrDefault();

        if (mapping == null)
            throw new InvalidOperationException(
                $"There are currently no designated mute roles within guild {guild.Id}");

        return guild.Roles.First(x => x.Id == mapping.Role.Id);
    }

    private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync(ulong guildId)
        => (await designatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = guildId, Type = DesignatedRoleType.Rank, IsDeleted = false,
                }))
            .Select(r => r.Role);

    private async Task RequireSubjectRankLowerThanModeratorRankAsync(ulong guildId, ulong moderatorId,
        ulong subjectId)
    {
        if (!await DoesModeratorOutrankUserAsync(guildId, moderatorId, subjectId))
            throw new InvalidOperationException(
                "Cannot moderate users that have a rank greater than or equal to your own.");
    }

    private async ValueTask RequireSubjectRankLowerThanModeratorRankAsync(IGuild guild, ulong moderatorId,
        IGuildUser? subject)
    {
        // If the subject is null, then the moderator automatically outranks them.
        if (subject is null)
            return;

        if (!await DoesModeratorOutrankUserAsync(guild, moderatorId, subject))
            throw new InvalidOperationException(
                "Cannot moderate users that have a rank greater than or equal to your own.");
    }

    private async Task<bool> DoesModeratorOutrankUserAsync(IGuild guild, ulong moderatorId, IGuildUser subject)
    {
        //If the subject is the guild owner, and the moderator is not the owner, the moderator does not outrank them
        if (guild.OwnerId == subject.Id && guild.OwnerId != moderatorId)
            return false;

        var moderator = await userService.GetGuildUserAsync(guild.Id, moderatorId);

        // If the moderator has the "Admin" permission, they outrank everyone in the guild but the owner
        if (moderator.GuildPermissions.Administrator)
            return true;

        var rankRoles = await GetRankRolesAsync(guild.Id);

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
            {InfractionType.Notice, AuthorizationClaim.ModerationNote},
            {InfractionType.Warning, AuthorizationClaim.ModerationWarn},
            {InfractionType.Mute, AuthorizationClaim.ModerationMute},
            {InfractionType.Ban, AuthorizationClaim.ModerationBan}
        };
}
