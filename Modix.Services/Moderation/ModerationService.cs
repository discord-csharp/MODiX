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
        Task AutoConfigureGuldAsync(IGuild guild);

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
        /// Retrieves the currently-configured mute role (the role that is assigned to users to mute them) for a given guild.
        /// </summary>
        /// <param name="guild">The guild whose mute role is to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the mute role currently configured for use within <paramref name="guild"/>.
        /// </returns>
        Task<IRole> GetMuteRoleAsync(IGuild guild);

        /// <summary>
        /// Sets the currently-configured mute role (the role that is assigned to users to mute them) for a given guild.
        /// </summary>
        /// <param name="guild">The guild whose mute role is to be set.</param>
        /// <param name="muteRole">The mute role to be used for <paramref name="guild"/>.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task SetMuteRoleAsync(IGuild guild, IRole muteRole);

        /// <summary>
        /// Retrieves the list of the Discord snowflake ID values of all the channels currently configured
        /// to receive logging messages from the moderation feature, for a given guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild whose logging channel ID values are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of channel ID values.
        /// </returns>
        Task<IReadOnlyCollection<ulong>> GetLogChannelIdsAsync(ulong guildId);

        /// <summary>
        /// Retrieves the list of all channels currently configured to receive logging messages from the moderation feature,
        /// for a given guild.
        /// </summary>
        /// <param name="guild">The guild whose logging channels are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of channels.
        /// </returns>
        Task<IReadOnlyCollection<IMessageChannel>> GetLogChannelsAsync(IGuild guild);

        /// <summary>
        /// Configures a channel to receive logging messages from the moderation feature, for a given guild.
        /// </summary>
        /// <param name="guild">The guild whose logging messages are to be sent to <paramref name="logChannel"/>.</param>
        /// <param name="logChannel">The channel to received logging messages from <paramref name="guild"/>.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddLogChannelAsync(IGuild guild, IMessageChannel logChannel);

        /// <summary>
        /// Configures a channel to stop receiving logging messages from the moderation feature, for a given guild.
        /// </summary>
        /// <param name="guild">The guild whose logging messages are being sent to <paramref name="logChannel"/>.</param>
        /// <param name="logChannel">The channel that should no longer receive logging messages from <paramref name="guild"/>.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveLogChannelAsync(IGuild guild, IMessageChannel logChannel);

        /// <summary>
        /// Creates an infraction upon a specified user, and logs an associated moderation action.
        /// </summary>
        /// <param name="type">The value to user for <see cref="InfractionEntity.Type"/>.<</param>
        /// <param name="subjectId">The value to use for <see cref="InfractionEntity.SubjectId"/>.</param>
        /// <param name="reason">The value to use for <see cref="ModerationActionEntity.Reason"/></param>
        /// <param name="duration">The value to use for <see cref="InfractionEntity.Duration"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration);

        /// <summary>
        /// Marks an existing, active, infraction of a given type, upon a given user, as rescinded.
        /// </summary>
        /// <param name="type">The <see cref="InfractionEntity.Type"/> value of the infraction to be rescinded.</param>
        /// <param name="subjectId">The <see cref="InfractionEntity.SubjectId"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(InfractionType type, ulong subjectId);

        /// <summary>
        /// Marks an existing infraction as rescinded, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(long infractionId);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task DeleteInfractionAsync(long infractionId);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of infractions.
        /// </returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriterias = null);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria, and returns a paged subset of the results, based on a given set of paging criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed, containing the requested set of infractions.</returns>
        Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        /// <summary>
        /// Retrieves a moderation action, based on its ID.
        /// </summary>
        /// <param name="moderationActionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested moderation action.
        /// </returns>
        Task<ModerationActionSummary> GetModerationActionSummaryAsync(long moderationActionId);

        /// <summary>
        /// Retrieves a collection of moderation actions, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which moderation actions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the moderation actions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of moderation actions.
        /// </returns>
        Task<IReadOnlyCollection<ModerationActionSummary>> SearchModerationActionsAsync(ModerationActionSearchCriteria searchCriteria);

        /// <summary>
        /// Retrieves a timestamp indicating the next time an existing infraction will be expiring.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing the requested timestamp value.
        /// </returns>
        Task<DateTimeOffset?> GetNextInfractionExpiration();
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
        /// Creates a new <see cref="ModerationService"/>.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="authorizationService">The value to use for <see cref="AuthorizationService"/>.</param>
        /// <param name="userService">The value to use for <see cref="UserService"/>.</param>
        /// <param name="moderationMuteRoleMappingRepository">The value to use for <see cref="ModerationMuteRoleMappingRepository"/>.</param>
        /// <param name="moderationActionRepository">The value to use for <see cref="ModerationActionRepository"/>.</param>
        /// <param name="infractionRepository">The value to use for <see cref="InfractionRepository"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for all parameters.</exception>
        public ModerationService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IGuildService guildService,
            IUserService userService,
            IModerationMuteRoleMappingRepository moderationMuteRoleMappingRepository,
            IModerationLogChannelMappingRepository moderationLogChannelMappingRepository,
            IModerationActionRepository moderationActionRepository,
            IInfractionRepository infractionRepository)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            GuildService = guildService ?? throw new ArgumentNullException(nameof(guildService));
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            ModerationMuteRoleMappingRepository = moderationMuteRoleMappingRepository ?? throw new ArgumentNullException(nameof(moderationMuteRoleMappingRepository));
            ModerationLogChannelMappingRepository = moderationLogChannelMappingRepository ?? throw new ArgumentNullException(nameof(moderationLogChannelMappingRepository));
            ModerationActionRepository = moderationActionRepository ?? throw new ArgumentNullException(nameof(moderationActionRepository));
            InfractionRepository = infractionRepository ?? throw new ArgumentNullException(nameof(infractionRepository));
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuldAsync(IGuild guild)
        {
            var muteRole = await GetOrCreateMuteRoleInGuildAsync(guild);

            foreach (var channel in await guild.GetChannelsAsync())
                await ConfigureChannelMuteRolePermissions(channel, muteRole);

            await CreateOrUpdateMuteRoleMapping(guild.Id, muteRole.Id);
        }

        /// <inheritdoc />
        public async Task AutoConfigureChannelAsync(IChannel channel)
        {
            if (channel is IGuildChannel guildChannel)
            {
                var muteRole = await GetOrCreateMuteRoleInGuildAsync(guildChannel.Guild);

                await ConfigureChannelMuteRolePermissions(guildChannel, muteRole);

                await CreateOrUpdateMuteRoleMapping(guildChannel.Guild.Id, muteRole.Id);
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
                await RescindInfractionAsync(expiredInfractionId);
        }

        /// <inheritdoc />
        public async Task UnConfigureGuildAsync(IGuild guild)
        {
            foreach(var mapping in await ModerationMuteRoleMappingRepository
                .SearchBriefsAsync(new ModerationMuteRoleMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    IsDeleted = false,
                }))
            {
                IDeletable muteRole = guild.Roles.FirstOrDefault(x => x.Id == mapping.MuteRoleId);
                if (muteRole != null)
                    await muteRole.DeleteAsync();

                await ModerationMuteRoleMappingRepository.TryDeleteAsync(mapping.Id, DiscordClient.CurrentUser.Id);
            }
        }

        /// <inheritdoc />
        public async Task<IRole> GetMuteRoleAsync(IGuild guild)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationConfigure);

            var mapping = await TryGetActiveMuteRoleMapping(guild.Id);

            if (mapping == null)
                throw new InvalidOperationException($"No mute role mapping exists for guild {guild.Id}");

            return guild.GetRole(mapping.MuteRoleId);
        }

        /// <inheritdoc />
        public Task SetMuteRoleAsync(IGuild guild, IRole muteRole)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationConfigure);

            return CreateOrUpdateMuteRoleMapping(guild.Id, muteRole.Id);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ulong>> GetLogChannelIdsAsync(ulong guildId)
             => (await ModerationLogChannelMappingRepository.SearchBriefsAsync(new ModerationLogChannelMappingSearchCriteria()
                {
                    GuildId = guildId,
                    IsDeleted = false
                }))
                .Select(x => x.LogChannelId)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IMessageChannel>> GetLogChannelsAsync(IGuild guild)
        {
            var mappings = await ModerationLogChannelMappingRepository.SearchBriefsAsync(new ModerationLogChannelMappingSearchCriteria()
            {
                GuildId = guild.Id,
                IsDeleted = false
            });

            return (await guild.GetChannelsAsync())
                .Where(x => mappings.Any(y => y.LogChannelId == x.Id))
                .Cast<IMessageChannel>()
                .ToArray();
        }

        /// <inheritdoc />
        public async Task AddLogChannelAsync(IGuild guild, IMessageChannel logChannel)
        {
            using (var transaction = await ModerationLogChannelMappingRepository.BeginCreateTransactionAsync())
            {
                if (await ModerationLogChannelMappingRepository.AnyAsync(new ModerationLogChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    LogChannelId = logChannel.Id,
                    IsDeleted = false
                }))
                {
                    throw new InvalidOperationException($"{logChannel.Name} already receives moderation log messages for {guild.Name}");
                }

                await ModerationLogChannelMappingRepository.CreateAsync(new ModerationLogChannelMappingCreationData()
                {
                    GuildId = guild.Id,
                    LogChannelId = logChannel.Id,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task RemoveLogChannelAsync(IGuild guild, IMessageChannel logChannel)
        {
            using(var transaction = await ModerationLogChannelMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await ModerationLogChannelMappingRepository.DeleteAsync(new ModerationLogChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    LogChannelId = logChannel.Id,
                    IsDeleted = false
                }, AuthorizationService.CurrentUserId.Value);

                if(deletedCount == 0)
                    throw new InvalidOperationException($"{logChannel.Name} is not currently receiving moderation log messages for {guild.Name}");

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(_createInfractionClaimsByType[type]);

            var guild = await GuildService.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);

            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

            if (((type == InfractionType.Notice) || (type == InfractionType.Warning))
                && string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException($"{type.ToString()} infractions require a reason to be given");

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
                        throw new InvalidOperationException($"Discord user {subjectId} already has an active {type} infraction");
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
                        await GetOrCreateMuteRoleInGuildAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.AddBanAsync(subject, reason: reason);
                    break;
            }
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(InfractionType type, ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

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
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(long infractionId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            await DoRescindInfractionAsync(
                await InfractionRepository.ReadSummaryAsync(infractionId));
        }

        /// <inheritdoc />
        public async Task DeleteInfractionAsync(long infractionId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationDelete);

            var infraction = await InfractionRepository.ReadSummaryAsync(infractionId);

            if (infraction == null)
                throw new InvalidOperationException($"Infraction {infractionId} does not exist");

            await InfractionRepository.TryDeleteAsync(infraction.Id, AuthorizationService.CurrentUserId.Value);

            var guild = await GuildService.GetGuildAsync(infraction.GuildId);
            var subject = await UserService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);

            switch (infraction.Type)
            {
                case InfractionType.Mute:
                    await subject.RemoveRoleAsync(
                        await GetOrCreateMuteRoleInGuildAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.RemoveBanAsync(subject);
                    break;
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return InfractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);
        }

        /// <inheritdoc />
        public Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return InfractionRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);
        }

        /// <inheritdoc />
        public Task<ModerationActionSummary> GetModerationActionSummaryAsync(long moderationActionId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return ModerationActionRepository.ReadSummaryAsync(moderationActionId);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ModerationActionSummary>> SearchModerationActionsAsync(ModerationActionSearchCriteria searchCriteria)
            => ModerationActionRepository.SearchSummariesAsync(searchCriteria);

        /// <inheritdoc />
        public Task<DateTimeOffset?> GetNextInfractionExpiration()
            => InfractionRepository.ReadExpiresFirstOrDefaultAsync(
                new InfractionSearchCriteria()
                {
                    IsRescinded = false,
                    IsDeleted = false,
                    ExpiresRange = new DateTimeOffsetRange()
                    {
                        From = DateTimeOffset.MinValue,
                        To = DateTimeOffset.MaxValue,
                    }
                },
                new []
                {
                    new SortingCriteria() { PropertyName = nameof(InfractionSummary.Expires), Direction = SortDirection.Ascending}
                });

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IGuildService"/> for interacting with discord guild within the application.
        /// </summary>
        internal protected IGuildService GuildService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IModerationMuteRoleMappingRepository"/> for storing and retrieving mute role configuration data.
        /// </summary>
        internal protected IModerationMuteRoleMappingRepository ModerationMuteRoleMappingRepository { get; }

        /// <summary>
        /// An <see cref="IModerationLogChannelMappingRepository"/> for storing and retrieving log channel configuration data.
        /// </summary>
        internal protected IModerationLogChannelMappingRepository ModerationLogChannelMappingRepository { get; }

        /// <summary>
        /// An <see cref="IModerationActionRepository"/> for storing and retrieving moderation action data.
        /// </summary>
        internal protected IModerationActionRepository ModerationActionRepository { get; }

        /// <summary>
        /// An <see cref="IInfractionRepository"/> for storing and retrieving infraction data.
        /// </summary>
        internal protected IInfractionRepository InfractionRepository { get; }

        private async Task<ModerationMuteRoleMappingBrief> TryGetActiveMuteRoleMapping(ulong guildId)
            => (await ModerationMuteRoleMappingRepository
                .SearchBriefsAsync(new ModerationMuteRoleMappingSearchCriteria()
                {
                    GuildId = guildId,
                    IsDeleted = false
                }))
                .FirstOrDefault();

        private async Task CreateOrUpdateMuteRoleMapping(ulong guildId, ulong muteRoleId)
        {
            using (var transaction = await ModerationMuteRoleMappingRepository.BeginCreateTransactionAsync())
            {
                var mapping = await TryGetActiveMuteRoleMapping(guildId);

                if (mapping != null)
                {
                    if (muteRoleId == mapping.MuteRoleId)
                        return;

                    await ModerationMuteRoleMappingRepository.TryDeleteAsync(mapping.Id, DiscordClient.CurrentUser.Id);
                }

                await ModerationMuteRoleMappingRepository.CreateAsync(new ModerationMuteRoleMappingCreationData()
                {
                    GuildId = guildId,
                    MuteRoleId = muteRoleId,
                    CreatedById = DiscordClient.CurrentUser.Id
                });

                transaction.Commit();
            }
        }

        private Task ConfigureChannelMuteRolePermissions(IGuildChannel channel, IRole muteRole)
        {
            // TODO: GetPermissionOverwrite and AddPermissionOverwriteAsync are bugged in Discord.NET 1.0.2.
            // Probably need to upgrade Discord.NET to get this functionality.
            return Task.CompletedTask;

            //var permissionOverwrite = channel.GetPermissionOverwrite(muteRole);
            //if (permissionOverwrite != null)
            //{
            //    if ((permissionOverwrite.Value.AllowValue == _mutePermissions.AllowValue) &&
            //        (permissionOverwrite.Value.DenyValue == _mutePermissions.DenyValue))
            //        return;

            //    await channel.RemovePermissionOverwriteAsync(muteRole);
            //}

            //await channel.AddPermissionOverwriteAsync(muteRole, _mutePermissions);
        }

        private async Task DoRescindInfractionAsync(InfractionSummary infraction)
        {
            if (infraction == null)
                throw new InvalidOperationException("Infraction does not exist");

            await InfractionRepository.TryRescindAsync(infraction.Id, AuthorizationService.CurrentUserId.Value);

            var guild = await GuildService.GetGuildAsync(infraction.GuildId);
            var subject = await UserService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);

            switch (infraction.Type)
            {
                case InfractionType.Mute:
                    await subject.RemoveRoleAsync(
                        await GetOrCreateMuteRoleInGuildAsync(guild));
                    break;

                case InfractionType.Ban:
                    await guild.RemoveBanAsync(subject);
                    break;

                default:
                    throw new InvalidOperationException($"{infraction.Type} infractions cannot be rescinded.");
            }
        }

        private async Task<IRole> GetOrCreateMuteRoleInGuildAsync(IGuild guild)
            => guild.Roles.FirstOrDefault(x => x.Name == MuteRoleName)
                ?? await guild.CreateRoleAsync(MuteRoleName);

        // Unused, because ConfigureChannelMuteRolePermissions is currently disabled.
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
