using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Serilog;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord channels, designated for use within the application.
    /// </summary>
    public interface IDesignatedChannelService
    {
        /// <summary>
        /// Assigns a channel to a given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild where the <paramref name="channel"/> exists.</param>
        /// <param name="channel">The channel to be assigned.</param>
        /// <param name="type">The type of designation to be assigned.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task<long> AddDesignatedChannelAsync(IGuild guild, IMessageChannel channel, DesignatedChannelType type);

        /// <summary>
        /// Unassigns a channel's previously given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild where the <paramref name="channel"/> exists.</param>
        /// <param name="channel">The channel to be unassigned.</param>
        /// <param name="type">The type of designation to be unassigned.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed, with the number of records deleted.</returns>
        Task<int> RemoveDesignatedChannelAsync(IGuild guild, IMessageChannel channel, DesignatedChannelType type);

        /// <summary>
        /// Removes a channel designation by ID.
        /// </summary>
        /// <param name="designationId">The ID of the designation to be removed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed, with the number of records deleted.</returns>
        Task<int> RemoveDesignatedChannelByIdAsync(long designationId);

        /// <summary>
        /// Checks whether any designated channels exist, for an arbitrary set of criteria.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild whose designated channels are to be checked.</param>
        /// <param name="type">The type of designated channels to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching channel designations exist.
        /// </returns>
        Task<bool> AnyDesignatedChannelAsync(ulong guildId, DesignatedChannelType type);

        /// <summary>
        /// Retrieves the Discord snowflake ID values of the channels assigned to a given designation, for a given guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild for which channel designations are to be retrieved.</param>
        /// <param name="type">The type of designation for which channels are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has compelted,
        /// containing the requested channel ID values.
        /// </returns>
        Task<IReadOnlyCollection<ulong>> GetDesignatedChannelIdsAsync(ulong guildId, DesignatedChannelType type);

        /// <summary>
        /// Retrieves the <see cref="IMessageChannel"/>s assigned to the given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild to retrieve designations for</param>
        /// <param name="type">The designation to retrieve assigned channels for</param>
        /// <returns>A <see cref="Task"/> that will complete when all <see cref="IMessageChannel"/>s have been retrieved.</returns>
        Task<IReadOnlyCollection<IMessageChannel>> GetDesignatedChannelsAsync(IGuild guild, DesignatedChannelType type);

        /// <summary>
        /// Retrieves a collection of <see cref="DesignatedChannelMappingBrief"/>s associated with the given guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild to retrieve designations for</param>
        /// <returns>A <see cref="Task"/> that will complete when all <see cref="DesignatedChannelMappingBrief"/>s have been retrieved.</returns>
        Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> GetDesignatedChannelsAsync(ulong guildId);

        /// <summary>
        /// Checks if the given channel has the given designation
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> where the channel is located</param>
        /// <param name="channel">The <see cref="IChannel"/> to check the designation for</param>
        /// <param name="designation">The <see cref="DesignatedChannelType"/> to check for</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns></returns>
        Task<bool> ChannelHasDesignationAsync(
            ulong guildId,
            ulong channelId,
            DesignatedChannelType designation,
            CancellationToken cancellationToken);

        /// <summary>
        /// Sends the given message (and embed) to the <see cref="IMessageChannel"/>s assigned to the given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to send the message to</param>
        /// <param name="designation">The <see cref="DesignatedChannelType"/> of the channels to send the messages to</param>
        /// <param name="content">The text content of the message</param>
        /// <param name="embed">An optional <see cref="Embed"/> to attach to the message</param>
        /// <returns>A <see cref="Task"/> that, when completed, results in a collection of the messages that were sent.</returns>
        Task<IReadOnlyCollection<IMessage>> SendToDesignatedChannelsAsync(IGuild guild, DesignatedChannelType designation, string content, Embed embed = null);
    }

    public class DesignatedChannelService : IDesignatedChannelService
    {
        internal protected IDesignatedChannelMappingRepository DesignatedChannelMappingRepository { get; }
        internal protected IAuthorizationService AuthorizationService { get; }

        public DesignatedChannelService(IDesignatedChannelMappingRepository designatedChannelMappingRepository, IAuthorizationService authorizationService)
        {
            DesignatedChannelMappingRepository = designatedChannelMappingRepository;
            AuthorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async Task<long> AddDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, DesignatedChannelType type)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingCreate);

            using (var transaction = await DesignatedChannelMappingRepository.BeginCreateTransactionAsync())
            {
                if (await DesignatedChannelMappingRepository.AnyAsync(new DesignatedChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    IsDeleted = false,
                    Type = type
                }, default))
                {
                    throw new InvalidOperationException($"{logChannel.Name} in {guild.Name} is already assigned to {type}");
                }

                var id = await DesignatedChannelMappingRepository.CreateAsync(new DesignatedChannelMappingCreationData()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    CreatedById = AuthorizationService.CurrentUserId.Value,
                    Type = type
                });

                transaction.Commit();

                return id;
            }
        }

        /// <inheritdoc />
        public async Task<int> RemoveDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, DesignatedChannelType type)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingDelete);

            using (var transaction = await DesignatedChannelMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await DesignatedChannelMappingRepository.DeleteAsync(new DesignatedChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    IsDeleted = false,
                    Type = type
                }, AuthorizationService.CurrentUserId.Value);

                if (deletedCount == 0)
                    throw new InvalidOperationException($"{logChannel.Name} in {guild.Name} is not assigned to {type}");

                transaction.Commit();
                return deletedCount;
            }
        }

        /// <inheritdoc />
        public async Task<int> RemoveDesignatedChannelByIdAsync(long designationId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingDelete);

            using (var transaction = await DesignatedChannelMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await DesignatedChannelMappingRepository.DeleteAsync(new DesignatedChannelMappingSearchCriteria()
                {
                    Id = designationId,
                    IsDeleted = false
                }, AuthorizationService.CurrentUserId.Value);

                if (deletedCount == 0)
                    throw new InvalidOperationException($"No designations with id {designationId} found.");

                transaction.Commit();
                return deletedCount;
            }
        }

        /// <inheritdoc />
        public Task<bool> AnyDesignatedChannelAsync(ulong guildId, DesignatedChannelType type)
            => DesignatedChannelMappingRepository.AnyAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                Type = type,
                IsDeleted = false
            }, default);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ulong>> GetDesignatedChannelIdsAsync(ulong guildId, DesignatedChannelType type)
            => DesignatedChannelMappingRepository.SearchChannelIdsAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                Type = type,
                IsDeleted = false
            });

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IMessageChannel>> GetDesignatedChannelsAsync(IGuild guild, DesignatedChannelType type)
        {
            var channelIds = await GetDesignatedChannelIdsAsync(guild.Id, type);

            if (!channelIds.Any())
                throw new InvalidOperationException($"{guild.Name} has no channels assigned to {type}");

            return (await Task.WhenAll(channelIds.Select(d => guild.GetChannelAsync(d))))
                .OfType<IMessageChannel>()
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IMessage>> SendToDesignatedChannelsAsync(IGuild guild, DesignatedChannelType designation, string text, Embed embed = null)
        {
            var channels = await GetDesignatedChannelsAsync(guild, designation);

            if (!channels.Any())
            {
                Log.Warning("Warning: Tried to send to channels assigned to designation {designation}, but none were assigned.", new { designation });
            }

            return await Task.WhenAll(channels.Select(channel => channel.SendMessageAsync(text, false, embed)));
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> GetDesignatedChannelsAsync(ulong guildId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedChannelMappingRead);

            return DesignatedChannelMappingRepository.SearchBriefsAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                IsDeleted = false
            });
        }

        /// <inheritdoc />
        public Task<bool> ChannelHasDesignationAsync(
                ulong guildId,
                ulong channelId,
                DesignatedChannelType designation,
                CancellationToken cancellationToken)
            => DesignatedChannelMappingRepository.AnyAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                Type = designation,
                ChannelId = channelId,
                IsDeleted = false
            }, cancellationToken);
    }
}
