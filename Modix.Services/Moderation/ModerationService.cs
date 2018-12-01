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
using Discord.Net;
using Modix.Common.ErrorHandling;
using Modix.Services.ErrorHandling;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Describes a service for performing moderation actions, within the application, within the context of a single incoming request.
    /// </summary>
    public interface IModerationService
    {
        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given channel.
        /// </summary>
        /// <param name="channel">The channel to be configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureChannelAsync(IChannel channel);

        /// <summary>
        /// Automatically rescinds any infractions that have expired.,
        /// based on <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoRescindExpiredInfractions();

        /// <summary>
        /// Removes all moderation configuration settings for a guild, by deleting all of its <see cref="ModerationMuteRoleMappingEntity"/> entries.
        /// </summary>
        /// <param name="guild">The guild to be un-configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task UnConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Creates an infraction upon a specified user, and logs an associated moderation action.
        /// </summary>
        /// <param name="type">The value to user for <see cref="InfractionEntity.Type"/>.<</param>
        /// <param name="subjectId">The value to use for <see cref="InfractionEntity.SubjectId"/>.</param>
        /// <param name="reason">The value to use for <see cref="ModerationActionEntity.Reason"/></param>
        /// <param name="duration">The value to use for <see cref="InfractionEntity.Duration"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task<ServiceResult> CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration);

        /// <summary>
        /// Marks an existing, active, infraction of a given type, upon a given user, as rescinded.
        /// </summary>
        /// <param name="type">The <see cref="InfractionEntity.Type"/> value of the infraction to be rescinded.</param>
        /// <param name="subjectId">The <see cref="InfractionEntity.SubjectId"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task<ServiceResult> RescindInfractionAsync(InfractionType type, ulong subjectId);

        /// <summary>
        /// Marks an existing infraction as rescinded, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <param name="isAutoRescind">
        /// Indicates whether the rescind request is an AutoRescind from MODiX.
        /// This determines whether checks such as rank validation will occur.
        /// </param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task<ServiceResult> RescindInfractionAsync(long infractionId, bool isAutoRescind = false);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task<ServiceResult> DeleteInfractionAsync(long infractionId);

        /// <summary>
        /// Deletes a message and creates a record of the deletion within the database.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <param name="reason">A description of the reason the message was deleted.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task<ServiceResult> DeleteMessageAsync(IMessage message, string reason);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of infractions.
        /// </returns>
        Task<ServiceResult<IReadOnlyCollection<InfractionSummary>>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriterias = null);


        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria, and returns a paged subset of the results, based on a given set of paging criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed, containing the requested set of infractions.</returns>
        Task<ServiceResult<RecordsPage<InfractionSummary>>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        /// <summary>
        /// Retrieves a count of the types of infractions the given user has recieved.
        /// </summary>
        /// <param name="subjectId">The ID of the user to retrieve counts for</param>
        /// <returns>A <see cref="Task"/> which results in a Dictionary of infraction type to counts. Will return zeroes for types for which there are no infractions.</returns>
        Task<ServiceResult<IDictionary<InfractionType, int>>> GetInfractionCountsForUserAsync(ulong subjectId);

        /// <summary>
        /// Retrieves a moderation action, based on its ID.
        /// </summary>
        /// <param name="moderationActionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested moderation action.
        /// </returns>
        Task<ServiceResult<ModerationActionSummary>> GetModerationActionSummaryAsync(long moderationActionId);

        /// <summary>
        /// Retrieves a collection of moderation actions, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which moderation actions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the moderation actions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of moderation actions.
        /// </returns>
        Task<ServiceResult<IReadOnlyCollection<ModerationActionSummary>>> SearchModerationActionsAsync(ModerationActionSearchCriteria searchCriteria);

        /// <summary>
        /// Retrieves a timestamp indicating the next time an existing infraction will be expiring.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing the requested timestamp value.
        /// </returns>
        Task<ServiceResult<DateTimeOffset>> GetNextInfractionExpiration();
    }

    /// <inheritdoc />
    public class ModerationService : IModerationService
    {
        /// <summary>
        /// The name to be used for the role in each guild that mutes users.
        /// </summary>
        // TODO: Push this to a bot-wide config? Or maybe on a per-guild basis, but with a bot-wide default, that's pulled from config?
        public const string MuteRoleName
            = "MODiX_Moderation_Mute";

        /// <summary>
        /// Creates a new <see cref="ModerationService"/>, with the given injected dependencies.
        /// </summary>
        public ModerationService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IChannelService channelService,
            IUserService userService,
            IModerationActionRepository moderationActionRepository,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            IInfractionRepository infractionRepository,
            IDeletedMessageRepository deletedMessageRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            ChannelService = channelService;
            UserService = userService;
            ModerationActionRepository = moderationActionRepository;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
            InfractionRepository = infractionRepository;
            DeletedMessageRepository = deletedMessageRepository;
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            var muteRole = await GetOrCreateDesignatedMuteRoleAsync(guild, AuthorizationService.CurrentUserId.Value);

            var nonCategoryChannels =
                (await guild.GetChannelsAsync())
                .Where(c => !(c is ICategoryChannel))
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
                var errorTemplate = "An exception was thrown when attempting to set up the mute role {Role} for guild {Guild}, channel #{Channel}. " +
                    "This is likely due to Modix not having the \"Manage Permissions\" permission - please check your server settings.";

                Log.Error(ex, errorTemplate, muteRole.Name, guild.Name, setUpChannels.Last().Name);

                return;
            }

            Log.Information("Successfully configured mute role @{MuteRole} for {ChannelCount} channels: {Channels}",
                muteRole.Name, nonCategoryChannels.Count, nonCategoryChannels.Select(c => c.Name));
        }

        /// <inheritdoc />
        public async Task AutoConfigureChannelAsync(IChannel channel)
        {
            if (channel is IGuildChannel guildChannel)
            {
                var muteRole = await GetOrCreateDesignatedMuteRoleAsync(guildChannel.Guild, AuthorizationService.CurrentUserId.Value);

                await ConfigureChannelMuteRolePermissionsAsync(guildChannel, muteRole);
            }
        }

        /// <inheritdoc />
        public async Task AutoRescindExpiredInfractions()
        {
            var expiredInfractionIds = await InfractionRepository.SearchIdsAsync(new InfractionSearchCriteria()
            {
                ExpiresRange = new DateTimeOffsetRange()
                {
                    To = DateTimeOffset.Now
                },
                IsRescinded = false,
                IsDeleted = false
            });

            foreach(var expiredInfractionId in expiredInfractionIds)
                await RescindInfractionAsync(expiredInfractionId, isAutoRescind: true);
        }

        /// <inheritdoc />
        public async Task UnConfigureGuildAsync(IGuild guild)
        {
            using (var transaction = await DesignatedRoleMappingRepository.BeginDeleteTransactionAsync())
            {
                foreach (var mapping in await DesignatedRoleMappingRepository
                    .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria()
                    {
                        GuildId = guild.Id,
                        Type = DesignatedRoleType.ModerationMute,
                        IsDeleted = false,
                    }))
                {
                    await DesignatedRoleMappingRepository.TryDeleteAsync(mapping.Id, AuthorizationService.CurrentUserId.Value);

                    var role = guild.Roles.FirstOrDefault(x => x.Id == mapping.Role.Id);
                    if ((role != null) && (role.Name == MuteRoleName) && (role is IDeletable deletable))
                        await deletable.DeleteAsync();
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration)
        {
            var authResult = AuthorizationService.CheckClaims(_createInfractionClaimsByType[type]);
            if (authResult.IsFailure) { return authResult; }

            var rankResult = await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, subjectId);
            if (rankResult.IsFailure) { return rankResult; }

            var guild = await DiscordClient.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);

            IGuildUser subject;

            if (!await UserService.GuildUserExistsAsync(guild.Id, subjectId))
            {
                subject = new EphemeralUser(subjectId, "[FORCED]", guild);
                await UserService.TrackUserAsync(subject);
            }
            else
            {
                subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);
            }

            if (type == InfractionType.Notice || type == InfractionType.Warning)
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return ServiceResult.FromError($"{type.ToString()} infractions require a reason to be given");
                }
            }

            var lengthResult = new InvalidLengthResult("Reason", reason.Length, maximum: 1000);
            if (lengthResult.IsFailure) { return lengthResult; }

            using (var transaction = await InfractionRepository.BeginCreateTransactionAsync())
            {
                if ((type == InfractionType.Mute) || (type == InfractionType.Ban))
                {
                    if (await InfractionRepository.AnyAsync(new InfractionSearchCriteria()
                    {
                        GuildId = guild.Id,
                        Types = new[] { type },
                        SubjectId = subject.Id,
                        IsRescinded = false,
                        IsDeleted = false
                    }))
                        return ServiceResult.FromError($"Discord user {subjectId} already has an active {type} infraction");
                }

                await InfractionRepository.CreateAsync(
                    new InfractionCreationData()
                    {
                        GuildId = guild.Id,
                        Type = type,
                        SubjectId = subjectId,
                        Reason = reason,
                        Duration = duration,
                        CreatedById = AuthorizationService.CurrentUserId.Value
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
                case InfractionType.Mute:
                    await subject.AddRoleAsync(
                        await GetDesignatedMuteRoleAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.AddBanAsync(subject, reason: reason);
                    break;
            }

            return ServiceResult.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> RescindInfractionAsync(InfractionType type, ulong subjectId)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRescind);
            if (authResult.IsFailure) { return authResult; }

            var rankResult = await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, subjectId);
            if (rankResult.IsFailure) { return rankResult; }

            await DoRescindInfractionAsync(
                (await InfractionRepository.SearchSummariesAsync(
                    new InfractionSearchCriteria()
                    {
                        GuildId = AuthorizationService.CurrentGuildId.Value,
                        Types = new [] { type },
                        SubjectId = subjectId,
                        IsRescinded = false,
                        IsDeleted = false,
                    }))
                    .FirstOrDefault());

            return ServiceResult.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> RescindInfractionAsync(long infractionId, bool isAutoRescind = false)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRescind);
            if (authResult.IsFailure) { return authResult; }

            await DoRescindInfractionAsync(
                await InfractionRepository.ReadSummaryAsync(infractionId), isAutoRescind);

            return ServiceResult.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteInfractionAsync(long infractionId)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationDeleteInfraction);
            if (authResult.IsFailure) { return authResult; }

            var infraction = await InfractionRepository.ReadSummaryAsync(infractionId);

            if (infraction == null)
                return ServiceResult.FromError($"Infraction {infractionId} does not exist");

            var rankResult = await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, infraction.Subject.Id);
            if (rankResult.IsFailure) { return rankResult; }

            await InfractionRepository.TryDeleteAsync(infraction.Id, AuthorizationService.CurrentUserId.Value);

            var guild = await DiscordClient.GetGuildAsync(infraction.GuildId);
            var subject = await UserService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);

            switch (infraction.Type)
            {
                case InfractionType.Mute:
                    await subject.RemoveRoleAsync(
                        await GetDesignatedMuteRoleAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.RemoveBanAsync(subject);
                    break;
            }

            return ServiceResult.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> DeleteMessageAsync(IMessage message, string reason)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationDeleteInfraction);
            if (authResult.IsFailure) { return authResult; }

            var rankResult = await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, message.Author.Id);
            if (rankResult.IsFailure) { return rankResult; }

            if (!(message.Channel is IGuildChannel guildChannel))
                return ServiceResult.FromError($"Cannot delete message {message.Id} because it is not a guild message");

            await UserService.TrackUserAsync(message.Author as IGuildUser);
            await ChannelService.TrackChannelAsync(guildChannel);

            using (var transaction = await DeletedMessageRepository.BeginCreateTransactionAsync())
            {
                await DeletedMessageRepository.CreateAsync(new DeletedMessageCreationData()
                {
                    GuildId = guildChannel.GuildId,
                    ChannelId = guildChannel.Id,
                    MessageId = message.Id,
                    AuthorId = message.Author.Id,
                    Content = message.Content,
                    Reason = reason,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                await message.DeleteAsync();

                transaction.Commit();
            }

            return ServiceResult.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IReadOnlyCollection<InfractionSummary>>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRead);
            var result = await InfractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);

            return ServiceResult.ShortCircuit(authResult, result);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<RecordsPage<InfractionSummary>>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRead);
            var result = await InfractionRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);

            return ServiceResult.ShortCircuit(authResult, result);
        }

        public async Task<ServiceResult<IDictionary<InfractionType, int>>> GetInfractionCountsForUserAsync(ulong subjectId)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRead);
            var result = await InfractionRepository.GetInfractionCountsAsync(new InfractionSearchCriteria
            {
                GuildId = AuthorizationService.CurrentGuildId,
                SubjectId = subjectId,
                IsDeleted = false
            });

            return ServiceResult.ShortCircuit(authResult, result);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ModerationActionSummary>> GetModerationActionSummaryAsync(long moderationActionId)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRead);
            var result = await ModerationActionRepository.ReadSummaryAsync(moderationActionId);

            return ServiceResult.ShortCircuit(authResult, result);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<IReadOnlyCollection<ModerationActionSummary>>> SearchModerationActionsAsync(ModerationActionSearchCriteria searchCriteria)
        {
            var authResult = AuthorizationService.CheckClaims(AuthorizationClaim.ModerationRead);
            var result = await ModerationActionRepository.SearchSummariesAsync(searchCriteria);

            return ServiceResult.ShortCircuit(authResult, result);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DateTimeOffset>> GetNextInfractionExpiration()
        {
            var result = await InfractionRepository.ReadExpiresFirstOrDefaultAsync(
                new InfractionSearchCriteria() {
                    IsRescinded = false,
                    IsDeleted = false,
                    ExpiresRange = new DateTimeOffsetRange() {
                        From = DateTimeOffset.MinValue,
                        To = DateTimeOffset.MaxValue,
                    }
                },
                new[]
                {
                    new SortingCriteria() { PropertyName = nameof(InfractionSummary.Expires), Direction = SortDirection.Ascending}
                });

            if (result == null)
            {
                return ServiceResult<DateTimeOffset>.FromError("No expiring infractions found.");
            }

            return ServiceResult.FromResult(result.Value);
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IChannelService"/> for interacting with discord channels within the application.
        /// </summary>
        internal protected IChannelService ChannelService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IModerationActionRepository"/> for storing and retrieving moderation action data.
        /// </summary>
        internal protected IModerationActionRepository ModerationActionRepository { get; }

        /// <summary>
        /// An <see cref="IInfractionRepository"/> for storing and retrieving infraction data.
        /// </summary>
        internal protected IInfractionRepository InfractionRepository { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleMappingRepository"/> storing and retrieving roles designated for use by the application.
        /// </summary>
        internal protected IDesignatedRoleMappingRepository DesignatedRoleMappingRepository { get; }

        /// <summary>
        /// An <see cref="IDeletedMessageRepository"/> for storing and retrieving records of deleted messages.
        /// </summary>
        internal protected IDeletedMessageRepository DeletedMessageRepository { get; }

        private async Task<IRole> GetOrCreateDesignatedMuteRoleAsync(IGuild guild, ulong currentUserId)
        {
            using (var transaction = await DesignatedRoleMappingRepository.BeginCreateTransactionAsync())
            {
                var mapping = (await DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    Type = DesignatedRoleType.ModerationMute,
                    IsDeleted = false
                })).FirstOrDefault();

                if (!(mapping is null))
                    return guild.Roles.First(x => x.Id == mapping.Role.Id);

                var role = guild.Roles.FirstOrDefault(x => x.Name == MuteRoleName)
                    ?? await guild.CreateRoleAsync(MuteRoleName);

                await DesignatedRoleMappingRepository.CreateAsync(new DesignatedRoleMappingCreationData()
                {
                    GuildId = guild.Id,
                    RoleId = role.Id,
                    Type = DesignatedRoleType.ModerationMute,
                    CreatedById = currentUserId
                });

                transaction.Commit();
                return role;
            }
        }

        private async Task ConfigureChannelMuteRolePermissionsAsync(IGuildChannel channel, IRole muteRole)
        {
            var permissionOverwrite = channel.GetPermissionOverwrite(muteRole);
            if (permissionOverwrite != null)
            {
                if ((permissionOverwrite.Value.AllowValue == _mutePermissions.AllowValue) &&
                    (permissionOverwrite.Value.DenyValue == _mutePermissions.DenyValue))
                {
                    Log.Debug("Skipping setting mute permissions for channel #{Channel} as they're already set.", channel.Name);
                    return;
                }

                Log.Debug("Removing permission overwrite for channel #{Channel}.", channel.Name);
                await channel.RemovePermissionOverwriteAsync(muteRole);
            }

            await channel.AddPermissionOverwriteAsync(muteRole, _mutePermissions);
            Log.Debug("Set mute permissions for role {Role} in channel #{Channel}.", muteRole.Name, channel.Name);
        }

        private async Task DoRescindInfractionAsync(InfractionSummary infraction, bool isAutoRescind = false)
        {
            if (infraction == null)
                throw new InvalidOperationException("Infraction does not exist");

            if (!isAutoRescind)
                await RequireSubjectRankLowerThanModeratorRankAsync(infraction.GuildId, infraction.Subject.Id);

            await InfractionRepository.TryRescindAsync(infraction.Id, AuthorizationService.CurrentUserId.Value);

            var guild = await DiscordClient.GetGuildAsync(infraction.GuildId);

            switch (infraction.Type)
            {
                case InfractionType.Mute:
                    if (!await UserService.GuildUserExistsAsync(guild.Id, infraction.Subject.Id))
                        throw new InvalidOperationException("Cannot unmute a user who is not in the server.");

                    var subject = await UserService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);
                    await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.RemoveBanAsync(infraction.Subject.Id);
                    break;

                default:
                    throw new InvalidOperationException($"{infraction.Type} infractions cannot be rescinded.");
            }
        }

        private async Task<IRole> GetDesignatedMuteRoleAsync(IGuild guild)
        {
            var mapping = (await DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guild.Id,
                Type = DesignatedRoleType.ModerationMute,
                IsDeleted = false
            })).FirstOrDefault();

            if (mapping == null)
                throw new InvalidOperationException($"There are currently no designated mute roles within guild {guild.Id}");

            return guild.Roles.First(x => x.Id == mapping.Role.Id);
        }

        private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync()
            => (await DesignatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = AuthorizationService.CurrentGuildId,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(r => r.Role);

        private async Task<ServiceResult> RequireSubjectRankLowerThanModeratorRankAsync(ulong guildId, ulong subjectId)
        {
            var moderator = await UserService.GetGuildUserAsync(guildId, AuthorizationService.CurrentUserId.Value);

            if (moderator.GuildPermissions.Administrator)
                return ServiceResult.FromSuccess();

            var rankRoles = await GetRankRolesAsync();

            var subject = await UserService.GetGuildUserAsync(guildId, subjectId);

            var subjectRankRoles = rankRoles.Where(r => subject.RoleIds.Contains(r.Id));
            var moderatorRankRoles = rankRoles.Where(r => moderator.RoleIds.Contains(r.Id));

            var greatestSubjectRank = subjectRankRoles.Any()
                ? subjectRankRoles.OrderByDescending(r => r.Position).FirstOrDefault()
                : null;

            var greatestModeratorRank = moderatorRankRoles.Any()
                ? moderatorRankRoles.OrderByDescending(r => r.Position).FirstOrDefault()
                : null;

            var greatestSubjectRankPosition = greatestSubjectRank?.Position ?? default(int?);
            var greatestModeratorRankPosition = greatestModeratorRank?.Position ?? default(int?);

            if (greatestModeratorRankPosition is null
                || greatestSubjectRankPosition >= greatestModeratorRankPosition)
            {
                return new InsufficientRankResult(greatestModeratorRank, greatestSubjectRank);
            }

            return ServiceResult.FromSuccess();
        }
            
        private static readonly OverwritePermissions _mutePermissions
            = new OverwritePermissions(
                sendMessages: PermValue.Deny,
                speak: PermValue.Deny);

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
            = new Dictionary<InfractionType, AuthorizationClaim>()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote },
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn },
                {InfractionType.Mute, AuthorizationClaim.ModerationMute },
                {InfractionType.Ban, AuthorizationClaim.ModerationBan }
            };
    }
}
