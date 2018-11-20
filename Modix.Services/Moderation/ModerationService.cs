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
    /// Does not perform any authorization checks.
    /// </summary>
    public interface IModerationService
    {
        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <param name="configuredById">The Discord snowflake ID of the user who is configuring the guild.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureGuildAsync(IGuild guild, ulong configuredById);

        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given channel.
        /// </summary>
        /// <param name="channel">The channel to be configured.</param>
        /// <param name="configuredById">The Discord snowflake ID of the user who is configuring the channel.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureChannelAsync(IChannel channel, ulong configuredById);

        /// <summary>
        /// Automatically rescinds any infractions that have expired.,
        /// based on <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        /// <param name="rescindedById">The Discord snowflake ID of the user who is rescinding the infractions.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoRescindExpiredInfractionsAsync(ulong rescindedById);

        /// <summary>
        /// Removes all moderation configuration settings for a guild, by deleting all of its <see cref="ModerationMuteRoleMappingEntity"/> entries.
        /// </summary>
        /// <param name="guild">The guild to be un-configured.</param>
        /// <param name="unconfiguredById">The Discord snowflake ID of the user who is un-configuring the guild.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task UnConfigureGuildAsync(IGuild guild, ulong unconfiguredById);

        /// <summary>
        /// Creates an infraction upon a specified user, and logs an associated moderation action.
        /// </summary>
        /// <param name="type">The value to user for <see cref="InfractionEntity.Type"/>.<</param>
        /// <param name="subjectId">The value to use for <see cref="InfractionEntity.SubjectId"/>.</param>
        /// <param name="reason">The value to use for <see cref="ModerationActionEntity.Reason"/></param>
        /// <param name="duration">The value to use for <see cref="InfractionEntity.Duration"/>.</param>
        /// <param name="guildId">The Discord snowflake ID of the guild for which the infraction is being created.</param>
        /// <param name="createdById">The Discord snowflake ID of the user who is creating the infraction.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration, ulong guildId, ulong createdById);

        /// <summary>
        /// Marks an existing, active, infraction of a given type, upon a given user, as rescinded.
        /// </summary>
        /// <param name="type">The <see cref="InfractionEntity.Type"/> value of the infraction to be rescinded.</param>
        /// <param name="subjectId">The <see cref="InfractionEntity.SubjectId"/> value of the infraction to be rescinded.</param>
        /// <param name="guildId">The Discord snowflake ID of the guild for which the infraction is being rescinded.</param>
        /// <param name="rescindedById">The Discord snowflake ID of the user who is rescinding the infraction.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(InfractionType type, ulong subjectId, ulong guildId, ulong rescindedById);

        /// <summary>
        /// Marks an existing infraction as deleted.
        /// </summary>
        /// <param name="infraction">The infraction to be deleted.</param>
        /// <param name="deletedById">The Discord snowflake ID of the user who is deleting the infraction.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task DeleteInfractionAsync(InfractionSummary infraction, ulong deletedById);

        /// <summary>
        /// Deletes a message and creates a record of the deletion within the database.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <param name="reason">A description of the reason the message was deleted.</param>
        /// <param name="deletedById">The Discord snowflake ID of the user who is deleting the infraction.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task DeleteMessageAsync(IMessage message, string reason, ulong deletedById);

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
        /// Retrieves a count of the types of infractions the given user has recieved.
        /// </summary>
        /// <param name="subjectId">The ID of the user to retrieve counts for</param>
        /// <param name="guildId">The Discord snowflake ID of the guild for which the infractions are being counted.</param>
        /// <returns>A <see cref="Task"/> which results in a Dictionary of infraction type to counts. Will return zeroes for types for which there are no infractions.</returns>
        Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId, ulong guildId);

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
        /// Creates a new <see cref="ModerationService"/>, with the given injected dependencies.
        /// </summary>
        public ModerationService(
            IDiscordClient discordClient,
            IChannelService channelService,
            IUserService userService,
            IModerationActionRepository moderationActionRepository,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            IInfractionRepository infractionRepository,
            IDeletedMessageRepository deletedMessageRepository)
        {
            DiscordClient = discordClient;
            ChannelService = channelService;
            UserService = userService;
            ModerationActionRepository = moderationActionRepository;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
            InfractionRepository = infractionRepository;
            DeletedMessageRepository = deletedMessageRepository;
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild, ulong configuredById)
        {
            var muteRole = await GetOrCreateDesignatedMuteRoleAsync(guild, configuredById);

            foreach (var channel in await guild.GetChannelsAsync())
                await ConfigureChannelMuteRolePermissions(channel, muteRole);
        }

        /// <inheritdoc />
        public async Task AutoConfigureChannelAsync(IChannel channel, ulong configuredById)
        {
            if (channel is IGuildChannel guildChannel)
            {
                var muteRole = await GetOrCreateDesignatedMuteRoleAsync(guildChannel.Guild, configuredById);

                await ConfigureChannelMuteRolePermissions(guildChannel, muteRole);
            }
        }

        /// <inheritdoc />
        public async Task AutoRescindExpiredInfractionsAsync(ulong rescindedById)
        {
            var expiredInfractions = await InfractionRepository.SearchSummariesAsync(new InfractionSearchCriteria()
            {
                ExpiresRange = new DateTimeOffsetRange()
                {
                    To = DateTimeOffset.Now
                },
                IsRescinded = false,
                IsDeleted = false
            });

            foreach (var expiredInfraction in expiredInfractions)
                await DoRescindInfractionAsync(expiredInfraction, rescindedById);
        }

        /// <inheritdoc />
        public async Task UnConfigureGuildAsync(IGuild guild, ulong unconfiguredById)
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
                    await DesignatedRoleMappingRepository.TryDeleteAsync(mapping.Id, unconfiguredById);

                    var role = guild.Roles.FirstOrDefault(x => x.Id == mapping.Role.Id);
                    if ((role != null) && (role.Name == MuteRoleName) && (role is IDeletable deletable))
                        await deletable.DeleteAsync();
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration, ulong guildId, ulong createdById)
        {
            var guild = await DiscordClient.GetGuildAsync(guildId);

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

            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

            if (reason.Length > 1000)
                throw new ArgumentException("Reason must be less than 1000 characters in length", nameof(reason));

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
                        CreatedById = createdById
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
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(InfractionType type, ulong subjectId, ulong guildId, ulong rescindedById)
        {
            await DoRescindInfractionAsync(
                (await InfractionRepository.SearchSummariesAsync(
                    new InfractionSearchCriteria()
                    {
                        GuildId = guildId,
                        Types = new [] { type },
                        SubjectId = subjectId,
                        IsRescinded = false,
                        IsDeleted = false,
                    }))
                    .FirstOrDefault(),
                rescindedById);
        }

        /// <inheritdoc />
        public async Task DeleteInfractionAsync(InfractionSummary infraction, ulong deletedById)
        {
            if (infraction is null)
                throw new InvalidOperationException($"Infraction {infraction} does not exist");

            await InfractionRepository.TryDeleteAsync(infraction.Id, deletedById);

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
        }

        /// <inheritdoc />
        public async Task DeleteMessageAsync(IMessage message, string reason, ulong deletedById)
        {
            if (!(message.Channel is IGuildChannel guildChannel))
                throw new InvalidOperationException($"Cannot delete message {message.Id} because it is not a guild message");

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
                    CreatedById = deletedById
                });

                await message.DeleteAsync();

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null)
            => await InfractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);

        public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId, ulong guildId)
            => await InfractionRepository.GetInfractionCountsAsync(new InfractionSearchCriteria
                {
                    GuildId = guildId,
                    SubjectId = subjectId,
                    IsDeleted = false
                });

        /// <inheritdoc />
        public async Task<ModerationActionSummary> GetModerationActionSummaryAsync(long moderationActionId)
            => await ModerationActionRepository.ReadSummaryAsync(moderationActionId);

        /// <inheritdoc />
        public async Task<DateTimeOffset?> GetNextInfractionExpiration()
            => await InfractionRepository.ReadExpiresFirstOrDefaultAsync(
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

        private async Task DoRescindInfractionAsync(InfractionSummary infraction, ulong rescindedById)
        {
            if (infraction is null)
                throw new InvalidOperationException("Infraction does not exist");
            
            await InfractionRepository.TryRescindAsync(infraction.Id, rescindedById);

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

            if (mapping is null)
                throw new InvalidOperationException($"There are currently no designated mute roles within guild {guild.Id}");

            return guild.Roles.First(x => x.Id == mapping.Role.Id);
        }
            
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
