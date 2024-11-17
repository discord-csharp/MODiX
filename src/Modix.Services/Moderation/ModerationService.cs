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
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Utilities;
using Modix.Services.MessageContentPatterns;

namespace Modix.Services.Moderation;

public class ModerationService(
    IDiscordClient discordClient,
    IChannelService channelService,
    IUserService userService,
    IScopedSession scopedSession,
    DesignatedChannelRelayService designatedChannelRelayService,
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
                    x.Id,
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

        await designatedChannelRelayService.RelayMessageToGuild(
            DesignatedChannelType.ModerationLog,
            infraction.GuildId,
            $"Infraction {infraction.Id} rescinded for reason: {reason}");
    }

    public async Task DeleteInfraction(long infractionId)
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
                    x.Id,
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

        await designatedChannelRelayService.RelayMessageToGuild(
            DesignatedChannelType.ModerationLog,
            infraction.GuildId,
            $"Infraction {infraction.Id} deleted");
    }

    public async Task DeleteMessage(IMessage message, string reason, ulong deletedById,
        CancellationToken cancellationToken)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationDeleteMessage);

        if (!hasClaim)
        {
            return;
        }

        if (message.Channel is not IGuildChannel guildChannel)
            throw new InvalidOperationException(
                $"Cannot delete message {message.Id} because it is not a guild message");

        var threadId = guildChannel is IThreadChannel threadChannel ? threadChannel.CategoryId : null;

        await userService.TrackUserAsync((IGuildUser)message.Author, cancellationToken);

        await channelService.TrackChannelAsync(guildChannel.Name,
            guildChannel.Id,
            guildChannel.GuildId,
            threadId, cancellationToken);

        var entity = new DeletedMessageEntity()
        {
            MessageId = message.Id,
            GuildId = guildChannel.GuildId,
            ChannelId = guildChannel.Id,
            AuthorId = message.Author.Id,
            Content = message.Content,
            Reason = reason,
            CreateAction = new ModerationActionEntity()
            {
                GuildId = guildChannel.GuildId,
                Type = ModerationActionType.MessageDeleted,
                Created = DateTimeOffset.UtcNow,
                CreatedById = deletedById
            }
        };

        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await designatedChannelRelayService.RelayMessageToGuild(
            DesignatedChannelType.ModerationLog,
            guildChannel.GuildId,
            $"Deleted message: {message.Content}");

        await message.DeleteAsync(new RequestOptions { CancelToken = cancellationToken });
    }

    public async Task DeleteMessages(ITextChannel channel, int count, bool skipOne,
        Func<Task<bool>> confirmDelegate)
    {
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

        await DoDeleteMessages(channel, guildChannel, messages);
    }

    public async Task DeleteMessages(ITextChannel channel, IGuildUser user, int count,
        Func<Task<bool>> confirmDelegate)
    {
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

        await DoDeleteMessages(channel, guildChannel, messages);
    }

    public async Task<RecordsPage<DeletedMessageSummary>> GetDeletedMessages(
        DeletedMessageSearchCriteria searchCriteria,
        IEnumerable<SortingCriteria> sortingCriteria,
        PagingCriteria pagingCriteria)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.LogViewDeletedMessages);

        if (!hasClaim)
        {
            return new RecordsPage<DeletedMessageSummary>();
        }

        var query = db
            .Set<DeletedMessageEntity>()
            .AsQueryable();

        if (searchCriteria.GuildId != null)
        {
            query = query.Where(x => x.GuildId == searchCriteria.GuildId);
        }

        if (searchCriteria.ChannelId != null)
        {
            query = query.Where(x => x.ChannelId == searchCriteria.ChannelId);
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Channel))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Channel.Name, searchCriteria.Channel));
        }

        if (searchCriteria.AuthorId != null)
        {
            query = query.Where(x => x.AuthorId == searchCriteria.AuthorId);
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Author))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Author.User.Username, searchCriteria.Author));
        }

        if (searchCriteria.CreatedById != null)
        {
            query = query.Where(x =>
                x.BatchId != null
                    ? x.Batch!.CreateAction.CreatedById == searchCriteria.CreatedById
                    : x.CreateAction.CreatedById == searchCriteria.CreatedById);
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.CreatedBy))
        {
            query = query.Where(x =>
                x.BatchId != null
                    ? ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Batch!.CreateAction.CreatedBy.User.Username,
                        searchCriteria.CreatedBy)
                    : ReusableQueries.DbCaseInsensitiveContains.Invoke(x.CreateAction.CreatedBy.User.Username,
                        searchCriteria.CreatedBy));
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Content))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Content, searchCriteria.Content));
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Reason))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Reason, searchCriteria.Reason));
        }

        if (searchCriteria.BatchId != null)
        {
            query = query.Where(x => x.BatchId == searchCriteria.BatchId);
        }

        var sourceQuery = query;

        var resultsQuery = query
            .SortByV3(sortingCriteria)
            .OrderThenBy(x => x.MessageId, SortDirection.Ascending)
            .PageBy(pagingCriteria)
            .Select(x => new DeletedMessageSummary
            {
                MessageId = x.MessageId,
                GuildId = x.GuildId,
                Content = x.Content,
                Reason = x.Reason,
                BatchId = x.BatchId,
                Created = x.CreateAction.Created,
                Channel = new GuildChannelBrief
                {
                    Id = x.ChannelId, Name = x.Channel.Name, ParentChannelId = x.Channel.ParentChannelId
                },
                Author = new GuildUserBrief
                {
                    Id = x.AuthorId,
                    Username = x.Author.User.Username,
                    Discriminator = x.Author.User.Discriminator,
                    Nickname = x.Author.Nickname,
                },
                CreatedBy = new GuildUserBrief
                {
                    Id = x.CreateAction.CreatedById,
                    Username = x.CreateAction.CreatedBy.User.Username,
                    Discriminator = x.CreateAction.CreatedBy.User.Discriminator,
                    Nickname = x.CreateAction.CreatedBy.Nickname,
                },
                Batch = x.BatchId != null
                    ? new DeletedMessageBatchBrief
                    {
                        Id = x.BatchId.Value,
                        CreateAction = new ModerationActionBrief
                        {
                            Id = x.Batch!.CreateAction.Id,
                            Created = x.Batch!.CreateAction.Created,
                            CreatedBy = new GuildUserBrief
                            {
                                Id = x.CreateAction.CreatedById,
                                Username = x.CreateAction.CreatedBy.User.Username,
                                Discriminator = x.CreateAction.CreatedBy.User.Discriminator,
                                Nickname = x.CreateAction.CreatedBy.Nickname,
                            }
                        }
                    }
                    : null
            });

        return new RecordsPage<DeletedMessageSummary>()
        {
            TotalRecordCount = await sourceQuery.LongCountAsync(),
            FilteredRecordCount = await resultsQuery.LongCountAsync(),
            Records = await resultsQuery.ToArrayAsync(),
        };
    }

    public async Task<IReadOnlyCollection<InfractionSummary>> SearchInfractions(InfractionSearchCriteria searchCriteria)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationRead);

        if (!hasClaim)
        {
            return [];
        }

        return await ProjectInfractionQuery(BuildInfractionQuery(searchCriteria)).ToListAsync();
    }

    public async Task<RecordsPage<InfractionSummary>> SearchInfractions(InfractionSearchCriteria searchCriteria,
        IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationRead);

        if (!hasClaim)
        {
            return new RecordsPage<InfractionSummary>();
        }

        var sourceQuery = BuildInfractionQuery(searchCriteria);

        var resultsQuery = sourceQuery
            .SortByV3(sortingCriteria)
            .OrderThenBy(x => x.Id, SortDirection.Ascending)
            .PageBy(pagingCriteria);

        var projected = ProjectInfractionQuery(resultsQuery);

        return new RecordsPage<InfractionSummary>()
        {
            TotalRecordCount = await sourceQuery.LongCountAsync(),
            FilteredRecordCount = await resultsQuery.LongCountAsync(),
            Records = await projected.ToArrayAsync(),
        };
    }

    private IQueryable<InfractionSummary> ProjectInfractionQuery(IQueryable<InfractionEntity> query)
    {
        return query.Select(x => new InfractionSummary
        {
            Id = x.Id,
            GuildId = x.GuildId,
            Type = x.Type,
            Reason = x.Reason,
            Duration = x.Duration,
            Subject =
                new GuildUserBrief()
                {
                    Id = x.Subject.User.Id,
                    Username = x.Subject.User.Username,
                    Discriminator = x.Subject.User.Discriminator,
                    Nickname = x.Subject.Nickname
                },
            CreateAction = new ModerationActionBrief()
            {
                Id = x.CreateAction.Id,
                Created = x.CreateAction.Created,
                CreatedBy = new GuildUserBrief()
                {
                    Id = x.CreateAction.CreatedBy.User.Id,
                    Username = x.CreateAction.CreatedBy.User.Username,
                    Discriminator = x.CreateAction.CreatedBy.User.Discriminator,
                    Nickname = x.CreateAction.CreatedBy.Nickname
                },
            },
            RescindAction = (x.RescindAction == null)
                ? null
                : new ModerationActionBrief()
                {
                    Id = x.RescindAction.Id,
                    Created = x.RescindAction.Created,
                    CreatedBy = new GuildUserBrief()
                    {
                        Id = x.RescindAction.CreatedBy.User.Id,
                        Username = x.RescindAction.CreatedBy.User.Username,
                        Discriminator = x.RescindAction.CreatedBy.User.Discriminator,
                        Nickname = x.RescindAction.CreatedBy.Nickname
                    }
                },
            DeleteAction = (x.DeleteAction == null)
                ? null
                : new ModerationActionBrief()
                {
                    Id = x.DeleteAction.Id,
                    Created = x.DeleteAction.Created,
                    CreatedBy = new GuildUserBrief()
                    {
                        Id = x.DeleteAction.CreatedBy.User.Id,
                        Username = x.DeleteAction.CreatedBy.User.Username,
                        Discriminator = x.DeleteAction.CreatedBy.User.Discriminator,
                        Nickname = x.DeleteAction.CreatedBy.Nickname
                    }
                },
            Expires = x.CreateAction.Created + x.Duration
        });
    }

    private IQueryable<InfractionEntity> BuildInfractionQuery(InfractionSearchCriteria searchCriteria)
    {
        var query = db
            .Set<InfractionEntity>()
            .AsQueryable();

        if (searchCriteria.Id is not null)
        {
            query = query.Where(x => x.Id == searchCriteria.Id);
        }

        if (searchCriteria.GuildId is not null)
        {
            query = query.Where(x => x.GuildId == searchCriteria.GuildId);
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Subject))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Subject.User.Username, searchCriteria.Subject));
        }

        if (searchCriteria.SubjectId is not null)
        {
            query = query.Where(x => x.SubjectId == searchCriteria.SubjectId);
        }

        if (searchCriteria.CreatedById is not null)
        {
            query = query.Where(x => x.CreateAction.CreatedById == searchCriteria.CreatedById);
        }

        if (!string.IsNullOrWhiteSpace(searchCriteria.Creator))
        {
            query = query.Where(x =>
                ReusableQueries.DbCaseInsensitiveContains.Invoke(x.CreateAction.CreatedBy.User.Username,
                    searchCriteria.Creator));
        }

        if (searchCriteria.Types is not null)
        {
            query = query.Where(x => searchCriteria.Types.Contains(x.Type));
        }

        if (searchCriteria.CreatedRange is not null)
        {
            query = query.Where(x =>
                x.CreateAction.Created >= searchCriteria.CreatedRange.Value.From
                && x.CreateAction.Created <= searchCriteria.CreatedRange.Value.To);
        }

        if (searchCriteria.ExpiresRange is not null)
        {
            query = query.Where(x => (x.CreateAction.Created + x.Duration) >= searchCriteria.ExpiresRange.Value.From
                                     && (x.CreateAction.Created + x.Duration) <= searchCriteria.ExpiresRange.Value.To);
        }

        if (searchCriteria.IsDeleted is not null)
        {
            query = query.Where(x =>
                searchCriteria.IsDeleted == true
                    ? x.DeleteActionId != null
                    : x.DeleteActionId == null);
        }

        if (searchCriteria.IsRescinded is not null)
        {
            query = query.Where(x =>
                searchCriteria.IsRescinded == true
                    ? x.RescindActionId != null
                    : x.RescindActionId == null);
        }

        return query;
    }

    public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationRead);

        if (!hasClaim)
        {
            return new Dictionary<InfractionType, int>();
        }

        var grouped = await db.Set<InfractionEntity>()
            .Where(x => x.GuildId == scopedSession.ExecutingGuildId)
            .Where(x => x.SubjectId == subjectId)
            .Where(x => x.DeleteActionId == null)
            .GroupBy(x => x.Type)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync();

        return Enum.GetValues<InfractionType>()
            .ToDictionary(x => x, x =>
                grouped.Where(y => y.Key == x).Sum(y => y.Count));
    }

    public async Task<ModerationActionSummary?> GetModerationAction(long moderationActionId)
    {
        return await db.Set<ModerationActionEntity>()
            .Where(x => x.Id == moderationActionId)
            .Select(entity => new ModerationActionSummary
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
                CreatedBy = new GuildUserBrief()
                {
                    Id = entity.CreatedBy.User.Id,
                    Username = entity.CreatedBy.User.Username,
                    Discriminator = entity.CreatedBy.User.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Infraction = (entity.Infraction == null)
                    ? null
                    : new InfractionBrief
                    {
                        Id = entity.Infraction.Id,
                        Type = entity.Infraction.Type,
                        Reason = entity.Infraction.Reason,
                        RescindReason = entity.Infraction.RescindReason,
                        Duration = entity.Infraction.Duration,
                        Subject = new GuildUserBrief
                        {
                            Id = entity.Infraction.Subject.User.Id,
                            Username = entity.Infraction.Subject.User.Username,
                            Discriminator = entity.Infraction.Subject.User.Discriminator,
                            Nickname = entity.Infraction.Subject.Nickname
                        }
                    },
                DeletedMessage = (entity.DeletedMessage == null)
                    ? null
                    : new DeletedMessageBrief()
                    {
                        Id = entity.DeletedMessage.MessageId,
                        Channel = new GuildChannelBrief
                        {
                            Id = entity.DeletedMessage.ChannelId,
                            Name = entity.DeletedMessage.Channel.Name,
                            ParentChannelId = entity.DeletedMessage.Channel.ParentChannelId
                        },
                        Author = new GuildUserBrief
                        {
                            Id = entity.DeletedMessage.Author.User.Id,
                            Username = entity.DeletedMessage.Author.User.Username,
                            Discriminator = entity.DeletedMessage.Author.User.Discriminator,
                            Nickname = entity.DeletedMessage.Author.Nickname,
                        },
                        Content = entity.DeletedMessage.Content,
                        Reason = entity.DeletedMessage.Reason,
                        BatchId = entity.DeletedMessage.BatchId,
                    },
                DeletedMessages = (entity.DeletedMessageBatchId == null)
                    ? null
                    : entity.DeletedMessageBatch!.DeletedMessages.Select(deletedMessage => new DeletedMessageBrief
                    {
                        Id = deletedMessage.MessageId,
                        Channel = new GuildChannelBrief
                        {
                            Id = deletedMessage.ChannelId,
                            Name = deletedMessage.Channel.Name,
                            ParentChannelId = deletedMessage.Channel.ParentChannelId,
                        },
                        Author = new GuildUserBrief
                        {
                            Id = deletedMessage.Author.User.Id,
                            Username = deletedMessage.Author.User.Username,
                            Discriminator = deletedMessage.Author.User.Discriminator,
                            Nickname = deletedMessage.Author.Nickname
                        },
                        Content = deletedMessage.Content,
                        Reason = deletedMessage.Reason,
                        BatchId = deletedMessage.BatchId,
                    }).ToArray(),
                OriginalInfractionReason = entity.OriginalInfractionReason,
            }).SingleAsync();
    }

    public async Task<DateTimeOffset?> GetNextInfractionExpiration()
    {
        return await db.Set<InfractionEntity>()
            .Where(x => x.DeleteActionId == null)
            .Where(x => x.RescindActionId == null)
            .OrderBy(x => x.CreateAction.Created + x.Duration)
            .Select(x => x.CreateAction.Created + x.Duration)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DoesModeratorOutrankUser(ulong guildId, ulong moderatorId, ulong subjectId)
    {
        //If the user doesn't exist in the guild, we outrank them
        if (await userService.GuildUserExistsAsync(guildId, subjectId) == false)
            return true;

        var subject = await userService.GetGuildUserAsync(guildId, subjectId);

        return await DoesModeratorOutrankUser(subject.Guild, moderatorId, subject);
    }

    public async Task<bool> HasActiveInfractions(ulong guildId, ulong userId, InfractionType? type = null)
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

    public async Task<(bool success, string? errorMessage)> UpdateInfraction(long infractionId,
        string newReason, ulong currentUserId)
    {
        var entity = await db.Set<InfractionEntity>()
            .Where(x => x.Id == infractionId)
            .SingleOrDefaultAsync();

        if (entity is null)
        {
            return (false, "No infraction found");
        }

        var hasClaim = await scopedSession.HasClaim(_createInfractionClaimsByType[entity.Type]);

        if (!hasClaim)
        {
            return (false, "No infraction claim found");
        }

        var createdByUserId = await db.Set<InfractionEntity>()
            .Where(x => x.Id == infractionId)
            .Select(x => x.CreateAction.CreatedById)
            .SingleAsync();

        if (createdByUserId != currentUserId)
        {
            var hasOtherEditClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationUpdateInfraction);

            if (!hasOtherEditClaim)
            {
                return (false, "No infraction claim found");
            }
        }

        entity.Reason = newReason;
        await db.SaveChangesAsync();

        return (true, null);
    }

    private async Task DoDeleteMessages(ITextChannel channel, IGuildChannel guildChannel,
        IEnumerable<IMessage> messages)
    {
        var hasClaim = await scopedSession.HasClaim(AuthorizationClaim.ModerationMassDeleteMessages);

        if (!hasClaim)
        {
            return;
        }

        await channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId,
            channel is IThreadChannel threadChannel ? threadChannel.CategoryId : null);

        var entity = new DeletedMessageBatchEntity()
        {
            CreateAction = new ModerationActionEntity()
            {
                Created = DateTimeOffset.UtcNow,
                CreatedById = scopedSession.ExecutingUserId,
                GuildId = scopedSession.ExecutingUserId,
                Type = ModerationActionType.MessageBatchDeleted,
            }
        };

        db.Add(entity);
        await db.SaveChangesAsync();

        var messageEntities = messages
            .Select(x => new DeletedMessageEntity
            {
                MessageId = x.Id,
                GuildId = scopedSession.ExecutingGuildId,
                ChannelId = x.Channel.Id,
                AuthorId = x.Author.Id,
                Content = x.Content,
                Reason = "Mass-deleted",
                BatchId = entity.Id,
            }).ToList();

        db.AddRange(messageEntities);
        await db.SaveChangesAsync();

        await channel.DeleteMessagesAsync(messages);

        await designatedChannelRelayService.RelayMessageToGuild(
            DesignatedChannelType.ModerationLog,
            guildChannel.GuildId,
            $"Mass-deleted messages in {channel.Name}");
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
            .Select(x => new { Id = x.RoleId, x.Role.Position })
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
