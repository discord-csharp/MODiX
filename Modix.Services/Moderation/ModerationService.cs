#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Serilog;
using System.Threading;

namespace Modix.Services.Moderation
{
    public interface IModerationService
    {
        Task AutoConfigureGuildAsync(IGuild guild);

        Task AutoConfigureChannelAsync(IChannel channel);

        Task AutoRescindExpiredInfractions();

        Task UnConfigureGuildAsync(IGuild guild);

        Task CreateInfractionAsync(ulong guildId, ulong moderatorId, InfractionType type, ulong subjectId,
            string reason, TimeSpan? duration);

        Task RescindInfractionAsync(long infractionId, string? reason = null);

        Task RescindInfractionAsync(InfractionType type, ulong guildId, ulong subjectId, string? reason = null);

        Task DeleteInfractionAsync(long infractionId);

        Task DeleteMessageAsync(IMessage message, string reason, ulong deletedById,
            CancellationToken cancellationToken);

        Task DeleteMessagesAsync(ITextChannel channel, int count, bool skipOne, Func<Task<bool>> confirmDelegate);

        Task DeleteMessagesAsync(ITextChannel channel, IGuildUser user, int count, Func<Task<bool>> confirmDelegate);

        Task<RecordsPage<DeletedMessageSummary>> SearchDeletedMessagesAsync(DeletedMessageSearchCriteria searchCriteria,
            IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria,
            IEnumerable<SortingCriteria>? sortingCriterias = null);

        Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria,
            IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId);

        Task<ModerationActionSummary?> GetModerationActionSummaryAsync(long moderationActionId);

        Task<DateTimeOffset?> GetNextInfractionExpiration();

        Task<bool> DoesModeratorOutrankUserAsync(ulong guildId, ulong moderatorId, ulong subjectId);

        Task<bool> AnyInfractionsAsync(InfractionSearchCriteria criteria);

        Task<IRole> GetOrCreateDesignatedMuteRoleAsync(ulong guildId, ulong currentUserId);

        Task<IRole> GetOrCreateDesignatedMuteRoleAsync(IGuild guild, ulong currentUserId);

        Task<(bool success, string? errorMessage)> UpdateInfractionAsync(long infractionId, string newReason,
            ulong currentUserId);
    }

    public class ModerationService : IModerationService
    {
        private readonly IDiscordClient _discordClient;
        private readonly IAuthorizationService _authorizationService;
        private readonly IChannelService _channelService;
        private readonly IUserService _userService;
        private readonly IModerationActionRepository _moderationActionRepository;
        private readonly IInfractionRepository _infractionRepository;
        private readonly IDesignatedRoleMappingRepository _designatedRoleMappingRepository;
        private readonly IDeletedMessageRepository _deletedMessageRepository;
        private readonly IDeletedMessageBatchRepository _deletedMessageBatchRepository;
        private readonly IRoleService _roleService;
        private readonly IDesignatedChannelService _designatedChannelService;

        // TODO: Push this to a bot-wide config? Or maybe on a per-guild basis, but with a bot-wide default, that's pulled from config?
        private const string MuteRoleName = "MODiX_Moderation_Mute";

        private const int MaxReasonLength = 1000;

        public ModerationService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IChannelService channelService,
            IUserService userService,
            IModerationActionRepository moderationActionRepository,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            IInfractionRepository infractionRepository,
            IDeletedMessageRepository deletedMessageRepository,
            IDeletedMessageBatchRepository deletedMessageBatchRepository,
            IRoleService roleService,
            IDesignatedChannelService designatedChannelService)
        {
            _discordClient = discordClient;
            _authorizationService = authorizationService;
            _channelService = channelService;
            _userService = userService;
            _moderationActionRepository = moderationActionRepository;
            _designatedRoleMappingRepository = designatedRoleMappingRepository;
            _infractionRepository = infractionRepository;
            _deletedMessageRepository = deletedMessageRepository;
            _deletedMessageBatchRepository = deletedMessageBatchRepository;
            _roleService = roleService;
            _designatedChannelService = designatedChannelService;
        }

        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

            await SetUpMuteRole(guild);
        }

        private async Task SetUpMuteRole(IGuild guild)
        {
            var muteRole = await GetOrCreateDesignatedMuteRoleAsync(guild, _authorizationService.CurrentUserId!.Value);

            var unmoderatedChannels =
                await _designatedChannelService.GetDesignatedChannelIdsAsync(guild.Id,
                    DesignatedChannelType.Unmoderated);

            var nonCategoryChannels =
                (await guild.GetChannelsAsync())
                .Where(c => c is (ITextChannel or IVoiceChannel) and not IThreadChannel)
                .Where(c => !unmoderatedChannels.Contains(c.Id))
                .ToList();

            var setUpChannels = new List<IGuildChannel>();

            try
            {
                foreach (var channel in nonCategoryChannels)
                {
                    setUpChannels.Add(channel);
                    await ConfigureChannelMuteRolePermissionsAsync(channel, muteRole);
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

        public async Task AutoConfigureChannelAsync(IChannel channel)
        {
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

            if (channel is IGuildChannel guildChannel)
            {
                var isUnmoderated = await _designatedChannelService.ChannelHasDesignationAsync(guildChannel.Guild.Id,
                    channel.Id, DesignatedChannelType.Unmoderated, default);

                if (isUnmoderated)
                {
                    return;
                }

                var muteRole =
                    await GetOrCreateDesignatedMuteRoleAsync(guildChannel.Guild,
                        _authorizationService.CurrentUserId.Value);

                await ConfigureChannelMuteRolePermissionsAsync(guildChannel, muteRole);
            }
        }

        public async Task AutoRescindExpiredInfractions()
        {
            var expiredInfractions = await _infractionRepository.SearchSummariesAsync(new InfractionSearchCriteria()
            {
                ExpiresRange = new DateTimeOffsetRange() { To = DateTimeOffset.UtcNow },
                IsRescinded = false,
                IsDeleted = false
            });

            foreach (var expiredInfraction in expiredInfractions)
            {
                await RescindInfractionAsync(expiredInfraction.Id, expiredInfraction.GuildId,
                    expiredInfraction.Subject.Id, isAutoRescind: true);
            }
        }

        public async Task UnConfigureGuildAsync(IGuild guild)
        {
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingDelete);

            using (var transaction = await _designatedRoleMappingRepository.BeginDeleteTransactionAsync())
            {
                foreach (var mapping in await _designatedRoleMappingRepository
                    .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria()
                    {
                        GuildId = guild.Id, Type = DesignatedRoleType.ModerationMute, IsDeleted = false,
                    }))
                {
                    await _designatedRoleMappingRepository.TryDeleteAsync(mapping.Id,
                        _authorizationService.CurrentUserId.Value);

                    var role = guild.Roles.FirstOrDefault(x => x.Id == mapping.Role.Id);
                    if ((role != null) && (role.Name == MuteRoleName) && (role is IDeletable deletable))
                        await deletable.DeleteAsync();
                }

                transaction.Commit();
            }
        }

        public async Task CreateInfractionAsync(ulong guildId, ulong moderatorId, InfractionType type, ulong subjectId,
            string reason, TimeSpan? duration)
        {
            _authorizationService.RequireClaims(_createInfractionClaimsByType[type]);

            if (reason is null)
                throw new ArgumentNullException(nameof(reason));

            if (reason.Length >= MaxReasonLength)
                throw new ArgumentException($"Reason must be less than {MaxReasonLength} characters in length",
                    nameof(reason));

            if (((type == InfractionType.Notice) || (type == InfractionType.Warning))
                && string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException($"{type.ToString()} infractions require a reason to be given");

            var guild = await _discordClient.GetGuildAsync(guildId);
            var subject = await _userService.TryGetGuildUserAsync(guild, subjectId, default);

            await RequireSubjectRankLowerThanModeratorRankAsync(guild, moderatorId, subject);

            using (var transaction = await _infractionRepository.BeginCreateTransactionAsync())
            {
                if ((type == InfractionType.Mute) || (type == InfractionType.Ban))
                {
                    if (await _infractionRepository.AnyAsync(new InfractionSearchCriteria()
                    {
                        GuildId = guildId,
                        Types = new[] {type},
                        SubjectId = subjectId,
                        IsRescinded = false,
                        IsDeleted = false
                    }))
                        throw new InvalidOperationException(
                            $"Discord user {subjectId} already has an active {type} infraction");
                }

                await _infractionRepository.CreateAsync(
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
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            var infraction = await _infractionRepository.ReadSummaryAsync(infractionId);
            await DoRescindInfractionAsync(infraction!.Type, infraction.GuildId, infraction.Subject.Id, infraction,
                reason);
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(InfractionType type, ulong guildId, ulong subjectId,
            string? reason = null)
        {
            _authorizationService.RequireAuthenticatedGuild();
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            if (reason?.Length >= MaxReasonLength)
                throw new ArgumentException($"Reason must be less than {MaxReasonLength} characters in length",
                    nameof(reason));

            var infraction = (await _infractionRepository.SearchSummariesAsync(
                new InfractionSearchCriteria()
                {
                    GuildId = _authorizationService.CurrentGuildId.Value,
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
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            var infraction = await _infractionRepository.ReadSummaryAsync(infractionId);

            await DoRescindInfractionAsync(infraction!.Type, guildId, subjectId, infraction, reason, isAutoRescind);
        }

        public async Task DeleteInfractionAsync(long infractionId)
        {
            _authorizationService.RequireAuthenticatedUser();
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationDeleteInfraction);

            var infraction = await _infractionRepository.ReadSummaryAsync(infractionId);

            if (infraction == null)
                throw new InvalidOperationException($"Infraction {infractionId} does not exist");

            await RequireSubjectRankLowerThanModeratorRankAsync(infraction.GuildId,
                _authorizationService.CurrentUserId.Value, infraction.Subject.Id);

            await _infractionRepository.TryDeleteAsync(infraction.Id, _authorizationService.CurrentUserId.Value);

            var guild = await _discordClient.GetGuildAsync(infraction.GuildId);

            switch (infraction.Type)
            {
                case InfractionType.Mute:

                    if (await _userService.GuildUserExistsAsync(guild.Id, infraction.Subject.Id))
                    {
                        var subject = await _userService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);
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

            await _userService.TrackUserAsync((IGuildUser)message.Author, cancellationToken);
            await _channelService.TrackChannelAsync(guildChannel.Name, guildChannel.Id, guildChannel.GuildId, guildChannel is IThreadChannel threadChannel ? threadChannel.CategoryId : null, cancellationToken);

            using var transaction = await _deletedMessageRepository.BeginCreateTransactionAsync(cancellationToken);

            await _deletedMessageRepository.CreateAsync(
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
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

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
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

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
            _authorizationService.RequireClaims(AuthorizationClaim.LogViewDeletedMessages);

            return await _deletedMessageRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria,
                pagingCriteria);
        }

        public Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(
            InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return _infractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);
        }

        public Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria,
            IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return _infractionRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);
        }

        public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await _infractionRepository.GetInfractionCountsAsync(new InfractionSearchCriteria
            {
                GuildId = _authorizationService.CurrentGuildId, SubjectId = subjectId, IsDeleted = false
            });
        }

        public Task<ModerationActionSummary?> GetModerationActionSummaryAsync(long moderationActionId)
        {
            return _moderationActionRepository.ReadSummaryAsync(moderationActionId);
        }

        public Task<DateTimeOffset?> GetNextInfractionExpiration()
            => _infractionRepository.ReadExpiresFirstOrDefaultAsync(
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
            if (await _userService.GuildUserExistsAsync(guildId, subjectId) == false)
                return true;

            var subject = await _userService.GetGuildUserAsync(guildId, subjectId);

            return await DoesModeratorOutrankUserAsync(subject.Guild, moderatorId, subject);
        }

        public async Task<bool> AnyInfractionsAsync(InfractionSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            return await _infractionRepository.AnyAsync(criteria);
        }

        public async Task<IRole> GetOrCreateDesignatedMuteRoleAsync(ulong guildId, ulong currentUserId)
        {
            var guild = await _discordClient.GetGuildAsync(guildId);
            return await GetOrCreateDesignatedMuteRoleAsync(guild, currentUserId);
        }

        public async Task<IRole> GetOrCreateDesignatedMuteRoleAsync(IGuild guild, ulong currentUserId)
        {
            using var transaction = await _designatedRoleMappingRepository.BeginCreateTransactionAsync();

            var mapping = (await _designatedRoleMappingRepository.SearchBriefsAsync(
                new DesignatedRoleMappingSearchCriteria()
                {
                    GuildId = guild.Id, Type = DesignatedRoleType.ModerationMute, IsDeleted = false
                })).FirstOrDefault();

            if (!(mapping is null))
                return guild.Roles.First(x => x.Id == mapping.Role.Id);

            var role = guild.Roles.FirstOrDefault(x => x.Name == MuteRoleName)
                       ?? await guild.CreateRoleAsync(MuteRoleName, isMentionable: false);

            await _roleService.TrackRoleAsync(role, default);

            await _designatedRoleMappingRepository.CreateAsync(new DesignatedRoleMappingCreationData()
            {
                GuildId = guild.Id,
                RoleId = role.Id,
                Type = DesignatedRoleType.ModerationMute,
                CreatedById = currentUserId
            });

            transaction.Commit();
            return role;
        }

        public async Task<(bool success, string? errorMessage)> UpdateInfractionAsync(long infractionId,
            string newReason, ulong currentUserId)
        {
            var infraction = await _infractionRepository.ReadSummaryAsync(infractionId);

            if (infraction is null)
                return (false, $"An infraction with an ID of {infractionId} could not be found.");

            _authorizationService.RequireClaims(_createInfractionClaimsByType[infraction.Type]);

            // Allow users who created the infraction to bypass any further
            // validation and update their own infraction
            if (infraction.CreateAction.CreatedBy.Id == currentUserId)
            {
                return (await _infractionRepository.TryUpdateAsync(infractionId, newReason, currentUserId), null);
            }

            // Else we know it's not the user's infraction
            _authorizationService.RequireClaims(AuthorizationClaim.ModerationUpdateInfraction);

            return (await _infractionRepository.TryUpdateAsync(infractionId, newReason, currentUserId), null);
        }

        private static async Task ConfigureChannelMuteRolePermissionsAsync(IGuildChannel channel, IRole muteRole)
        {
            try
            {
                var permissionOverwrite = channel.GetPermissionOverwrite(muteRole);

                if (permissionOverwrite is null || _mutePermissions.ToDenyList().Any(x => !permissionOverwrite.GetValueOrDefault().ToDenyList().Contains(x)))
                {
                    await channel.AddPermissionOverwriteAsync(muteRole, _mutePermissions, new() { AuditLogReason = "Setting mute role permissions." });
                    Log.Debug("Set mute permissions for role {Role} in channel #{Channel}.", muteRole.Name, channel.Name);
                }
                else
                {
                    Log.Debug("Skipping setting mute permissions for channel #{Channel} as they're already set.", channel.Name);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed setting channel mute role on #{Channel}", channel.Name);
                throw;
            }
        }

        private async Task DoDeleteMessagesAsync(ITextChannel channel, IGuildChannel guildChannel,
            IEnumerable<IMessage> messages)
        {
            await channel.DeleteMessagesAsync(messages);

            using var transaction = await _deletedMessageBatchRepository.BeginCreateTransactionAsync();
            await _channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId, channel is IThreadChannel threadChannel ? threadChannel.CategoryId : null);

            await _deletedMessageBatchRepository.CreateAsync(new DeletedMessageBatchCreationData()
            {
                CreatedById = _authorizationService.CurrentUserId!.Value,
                GuildId = _authorizationService.CurrentGuildId!.Value,
                Data = messages.Select(
                    x => new DeletedMessageCreationData()
                    {
                        AuthorId = x.Author.Id,
                        ChannelId = x.Channel.Id,
                        Content = x.Content,
                        GuildId = _authorizationService.CurrentGuildId.Value,
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
                await RequireSubjectRankLowerThanModeratorRankAsync(guildId, _authorizationService.CurrentUserId!.Value,
                    subjectId);
            }

            var guild = await _discordClient.GetGuildAsync(guildId);

            switch (type)
            {
                case InfractionType.Mute:
                    if (!await _userService.GuildUserExistsAsync(guild.Id, subjectId))
                    {
                        Log.Information(
                            "Attempted to remove the mute role from {0} ({1}), but they were not in the server.",
                            infraction?.Subject.GetFullUsername() ?? "Unknown user",
                            subjectId);
                        break;
                    }

                    var subject = await _userService.GetGuildUserAsync(guild.Id, subjectId);
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
                await _infractionRepository.TryRescindAsync(infraction.Id, _authorizationService.CurrentUserId!.Value,
                    reason);
            }
        }

        private async Task<IRole> GetDesignatedMuteRoleAsync(IGuild guild)
        {
            var mapping = (await _designatedRoleMappingRepository.SearchBriefsAsync(
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
            => (await _designatedRoleMappingRepository
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

            var moderator = await _userService.GetGuildUserAsync(guild.Id, moderatorId);

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

        private static readonly OverwritePermissions _mutePermissions
            = new(
                addReactions: PermValue.Deny,
                requestToSpeak: PermValue.Deny,
                sendMessages: PermValue.Deny,
                sendMessagesInThreads: PermValue.Deny,
                speak: PermValue.Deny,
                usePrivateThreads: PermValue.Deny,
                usePublicThreads: PermValue.Deny);

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
            = new()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote},
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn},
                {InfractionType.Mute, AuthorizationClaim.ModerationMute},
                {InfractionType.Ban, AuthorizationClaim.ModerationBan}
            };
    }
}
