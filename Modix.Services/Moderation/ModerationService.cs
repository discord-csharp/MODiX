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
        /// <param name="isAutoRescind">
        /// Indicates whether the rescind request is an AutoRescind from MODiX.
        /// This determines whether checks such as rank validation will occur.
        /// </param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(long infractionId, bool isAutoRescind = false);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task DeleteInfractionAsync(long infractionId);

        /// <summary>
        /// Deletes a message and creates a record of the deletion within the database.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <param name="reason">A description of the reason the message was deleted.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task DeleteMessageAsync(IMessage message, string reason);

        /// <summary>
        /// Mass-deletes a specified number of messages.
        /// </summary>
        /// <param name="channel">The channel in which the messages are to be deleted.</param>
        /// <param name="count">The number of messages to delete.</param>
        /// <param name="skipOne">Indicates whether to skip one message (i.e. the command message) before deleting.</param>
        /// <param name="confirmDelegate">A delegate that is invoked to confirm whether to proceed with the operation.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task DeleteMessagesAsync(ITextChannel channel, int count, bool skipOne, Func<Task<bool>> confirmDelegate);

        /// <summary>
        /// Mass-deletes a specified number of messages.
        /// </summary>
        /// <param name="channel">The channel in which the messages are to be deleted.</param>
        /// <param name="user">The user whose messages are to be deleted.</param>
        /// <param name="count">The number of messages to delete.</param>
        /// <param name="confirmDelegate">A delegate that is invoked to confirm whether to proceed with the operation.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task DeleteMessagesAsync(ITextChannel channel, IGuildUser user, int count, Func<Task<bool>> confirmDelegate);

        /// <summary>
        /// Retrieves a collection of deleted messages based on a given set of criteria, and returns a paged subset of the results based on a given set of paging criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which deleted messages are to be returned.</param>
        /// <param name="sortingCriteria">The criteria defining how to sort the deleted messages to be returned.</param>
        /// <param name="pagingCriteria">The criteria defining how to page the deleted messages to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed, containing the requested set of deleted messages.</returns>
        Task<RecordsPage<DeletedMessageSummary>> SearchDeletedMessagesAsync(DeletedMessageSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

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
        /// Retrieves a count of the types of infractions the given user has recieved.
        /// </summary>
        /// <param name="subjectId">The ID of the user to retrieve counts for</param>
        /// <returns>A <see cref="Task"/> which results in a Dictionary of infraction type to counts. Will return zeroes for types for which there are no infractions.</returns>
        Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId);

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

        /// <summary>
        /// Determines whether the supplied moderator outranks the supplied subject.
        /// </summary>
        /// <param name="guildId">The guild in which the moderation would be performed.</param>
        /// <param name="moderatorId">The moderator that would perform the moderation.</param>
        /// <param name="subjectId">The subject of the moderation.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing a flag indicating whether the moderator outranks the subject.
        /// </returns>
        Task<bool> DoesModeratorOutrankUserAsync(ulong guildId, ulong moderatorId, ulong subjectId);

        /// <summary>
        /// Determines whether there are any infractions that meet the supplied criteria.
        /// </summary>
        /// <param name="criteria">The criteria defining which infractions are to be searched.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operaiton is complete,
        /// with a flag indicating whether there are any infractions meeting the supplied criteria.
        /// </returns>
        Task<bool> AnyInfractionsAsync(InfractionSearchCriteria criteria);

        /// <summary>
        /// Retrieves the designated mute role for the supplied guild or creates the role if it does not
        /// already exist.
        /// </summary>
        /// <param name="guild">The guild to which the mute role belongs.</param>
        /// <param name="currentUserId">The Discord snowflake ID of the current user.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// containing the designated mute role in the guild.
        /// </returns>
        Task<IRole> GetOrCreateDesignatedMuteRoleAsync(IGuild guild, ulong currentUserId);
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
            IDeletedMessageRepository deletedMessageRepository,
            IDeletedMessageBatchRepository deletedMessageBatchRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            ChannelService = channelService;
            UserService = userService;
            ModerationActionRepository = moderationActionRepository;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
            InfractionRepository = infractionRepository;
            DeletedMessageRepository = deletedMessageRepository;
            DeletedMessageBatchRepository = deletedMessageBatchRepository;
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

            await SetUpMuteRole(guild);
            await SetUpMentionableRoles(guild);
        }

        private async Task SetUpMentionableRoles(IGuild guild)
        {
            var mentionableRoleMappings = await DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
            {
                GuildId = guild.Id,
                Type = DesignatedRoleType.RestrictedMentionability,
                IsDeleted = false
            });

            Log.Information("Roles with RestrictedMentionability are: {Roles}", mentionableRoleMappings.Select(d => d.Role.Name));

            //Get the actual roles that correspond to our mappings
            var mentionableRoles = mentionableRoleMappings
                .Join(guild.Roles, d => d.Role.Id, d => d.Id, (map, role) => role)
                .Where(d => d.IsMentionable);

            try
            {
                //Ensure all roles with restricted mentionability are not mentionable
                foreach (var role in mentionableRoles)
                {
                    Log.Information("Role @{Role} has RestrictedMentionability but was marked as Mentionable.", role.Name);
                    await role.ModifyAsync(d => d.Mentionable = false);
                    Log.Information("Role @{Role} was set to unmentionable.", role.Name);
                }
            }
            catch (HttpException ex)
            {
                var errorTemplate = "An exception was thrown when attempting to set up mention-restricted roles for {Guild}. " +
                    "This is likely due to Modix not having the \"Manage Roles\" permission, or Modix's role being below one of " +
                    "the mention-restricted roles in your server's Role list - please check your server settings.";

                Log.Error(ex, errorTemplate, guild.Name);
            }
        }

        private async Task SetUpMuteRole(IGuild guild)
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
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

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
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingDelete);

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
        public async Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(_createInfractionClaimsByType[type]);

            await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, subjectId);

            var guild = await DiscordClient.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);

            IGuildUser subject;

            if (!await UserService.GuildUserExistsAsync(guild.Id, subjectId))
            {
                subject = await UserService.GetUserInformationAsync(guild.Id, subjectId);

                if (subject == null)
                    throw new InvalidOperationException($"The given subject was not valid, ID: {subjectId}");

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
                        Types = new[] { type },
                        SubjectId = subjectId,
                        IsRescinded = false,
                        IsDeleted = false,
                    }))
                    .FirstOrDefault());
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(long infractionId, bool isAutoRescind = false)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            await DoRescindInfractionAsync(
                await InfractionRepository.ReadSummaryAsync(infractionId), isAutoRescind);
        }

        /// <inheritdoc />
        public async Task DeleteInfractionAsync(long infractionId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationDeleteInfraction);

            var infraction = await InfractionRepository.ReadSummaryAsync(infractionId);

            if (infraction == null)
                throw new InvalidOperationException($"Infraction {infractionId} does not exist");

            await RequireSubjectRankLowerThanModeratorRankAsync(infraction.GuildId, infraction.Subject.Id);

            await InfractionRepository.TryDeleteAsync(infraction.Id, AuthorizationService.CurrentUserId.Value);

            var guild = await DiscordClient.GetGuildAsync(infraction.GuildId);

            switch (infraction.Type)
            {
                case InfractionType.Mute:

                    if (await UserService.GuildUserExistsAsync(guild.Id, infraction.Subject.Id))
                    {
                        var subject = await UserService.GetGuildUserAsync(guild.Id, infraction.Subject.Id);
                        await subject.RemoveRoleAsync(await GetDesignatedMuteRoleAsync(guild));
                    }
                    else
                    {
                        Log.Warning("Tried to unmute {User} while deleting mute infraction, but they weren't in the guild: {Guild}",
                            infraction.Subject.Id, guild.Id);
                    }
                    
                    break;

                case InfractionType.Ban:
                    await guild.RemoveBanAsync(infraction.Subject.Id);
                    break;
            }
        }

        /// <inheritdoc />
        public async Task DeleteMessageAsync(IMessage message, string reason)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationDeleteMessage);

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
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                await message.DeleteAsync();

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task DeleteMessagesAsync(ITextChannel channel, int count, bool skipOne, Func<Task<bool>> confirmDelegate)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

            if (confirmDelegate is null)
                throw new ArgumentNullException(nameof(confirmDelegate));

            if (!(channel is IGuildChannel guildChannel))
                throw new InvalidOperationException($"Cannot delete messages in {channel.Name} because it is not a guild channel.");

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

        /// <inheritdoc />
        public async Task DeleteMessagesAsync(ITextChannel channel, IGuildUser user, int count, Func<Task<bool>> confirmDelegate)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationMassDeleteMessages);

            if (confirmDelegate is null)
                throw new ArgumentNullException(nameof(confirmDelegate));

            if (!(channel is IGuildChannel guildChannel))
                throw new InvalidOperationException($"Cannot delete messages in {channel.Name} because it is not a guild channel.");

            var confirmed = await confirmDelegate();

            if (!confirmed)
                return;

            var clampedCount = Math.Clamp(count, 0, 100);

            if (clampedCount == 0)
                return;

            var messages = (await channel.GetMessagesAsync(100).FlattenAsync()).Where(x => x.Author.Id == user.Id).Take(clampedCount);

            await DoDeleteMessagesAsync(channel, guildChannel, messages);
        }

        /// <inheritdoc />
        public async Task<RecordsPage<DeletedMessageSummary>> SearchDeletedMessagesAsync(DeletedMessageSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.LogViewDeletedMessages);

            return await DeletedMessageRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);
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

        public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.GetInfractionCountsAsync(new InfractionSearchCriteria
            {
                GuildId = AuthorizationService.CurrentGuildId,
                SubjectId = subjectId,
                IsDeleted = false
            });
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

        public async Task<bool> DoesModeratorOutrankUserAsync(ulong guildId, ulong moderatorId, ulong subjectId)
        {
            var moderator = await UserService.GetGuildUserAsync(guildId, moderatorId);

            //If the user doesn't exist in the guild, we outrank them
            if (await UserService.GuildUserExistsAsync(guildId, subjectId) == false)
                return true;

            var subject = await UserService.GetGuildUserAsync(guildId, subjectId);

            //If the subject is the guild owner, and we are not the owner, we do not outrank them
            if (subject.Guild.OwnerId == subjectId && subject.Guild.OwnerId != moderatorId)
                return false;

            //If we have the "Admin" permission, we outrank everyone in the guild but the owner
            if (moderator.GuildPermissions.Administrator)
                return true;

            var rankRoles = await GetRankRolesAsync(guildId);

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

        public async Task<bool> AnyInfractionsAsync(InfractionSearchCriteria criteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            return await InfractionRepository.AnyAsync(criteria);
        }

        public async Task<IRole> GetOrCreateDesignatedMuteRoleAsync(IGuild guild, ulong currentUserId)
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

        /// <summary>
        /// An <see cref="IDeletedMessageBatchRepository"/> for storing and retrieving records of deleted message batches.
        /// </summary>
        internal protected IDeletedMessageBatchRepository DeletedMessageBatchRepository { get; }

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

        private async Task DoDeleteMessagesAsync(ITextChannel channel, IGuildChannel guildChannel, IEnumerable<IMessage> messages)
        {
            await channel.DeleteMessagesAsync(messages);

            using (var transaction = await DeletedMessageBatchRepository.BeginCreateTransactionAsync())
            {
                await ChannelService.TrackChannelAsync(guildChannel);

                await DeletedMessageBatchRepository.CreateAsync(new DeletedMessageBatchCreationData()
                {
                    CreatedById = AuthorizationService.CurrentUserId.Value,
                    GuildId = AuthorizationService.CurrentGuildId.Value,
                    Data = messages.Select(
                        x => new DeletedMessageCreationData()
                        {
                            AuthorId = x.Author.Id,
                            ChannelId = x.Channel.Id,
                            Content = x.Content,
                            GuildId = AuthorizationService.CurrentGuildId.Value,
                            MessageId = x.Id,
                            Reason = "Mass-deleted.",
                        }),
                });

                transaction.Commit();
            }
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

        private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync(ulong guildId)
            => (await DesignatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = guildId,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(r => r.Role);

        private async Task RequireSubjectRankLowerThanModeratorRankAsync(ulong guildId, ulong subjectId)
        {
            if (!await DoesModeratorOutrankUserAsync(guildId, AuthorizationService.CurrentUserId.Value, subjectId))
                throw new InvalidOperationException("Cannot moderate users that have a rank greater than or equal to your own.");
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
