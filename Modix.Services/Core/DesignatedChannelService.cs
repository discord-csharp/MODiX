using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Serilog;

namespace Modix.Services.Core
{
    public interface IDesignatedChannelService
    {
        /// <summary>
        /// Assigns a channel to a given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild where the <paramref name="logChannel"/> exists.</param>
        /// <param name="logChannel">The channel to be assigned.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, ChannelDesignation designation);

        /// <summary>
        /// Unassigns a channel's previously given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild where the <paramref name="logChannel"/> exists.</param>
        /// <param name="logChannel">The channel to be unassigned.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, ChannelDesignation designation);

        /// <summary>
        /// Retrieves the <see cref="IMessageChannel"/>s assigned to the given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild to retrieve designations for</param>
        /// <param name="designation">The designation to retrieve assigned channels for</param>
        /// <returns>A <see cref="Task"/> that will complete when all <see cref="IMessageChannel"/>s have been retrieved.</returns>
        Task<IEnumerable<IMessageChannel>> GetDesignatedChannels(IGuild guild, ChannelDesignation designation);

        /// <summary>
        /// Retrieves a collection of <see cref="ulong"/>s corresponding to the channels assigned to a given designation, for a given guild id.
        /// </summary>
        /// <param name="guild">The ID of the guild to retrieve designations for</param>
        /// <param name="designation">The designation to retrieve assigned channels for</param>
        /// <returns>A <see cref="Task"/> that will complete when all <see cref="IMessageChannel"/>s have been retrieved.</returns>
        Task<IEnumerable<ulong>> GetDesignatedChannelIds(ulong guildId, ChannelDesignation designation);

        /// <summary>
        /// Retrieves a collection of <see cref="DesignatedChannelMappingBrief"/>s associated with the given guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild to retrieve designations for</param>
        /// <returns>A <see cref="Task"/> that will complete when all <see cref="DesignatedChannelMappingBrief"/>s have been retrieved.</returns>
        Task<IEnumerable<DesignatedChannelMappingBrief>> GetDesignatedChannels(ulong guildId);

        /// <summary>
        /// Checks if the given channel has the given designation
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> where the channel is located</param>
        /// <param name="channel">The <see cref="IChannel"/> to check the designation for</param>
        /// <param name="designation">The <see cref="ChannelDesignation"/> to check for</param>
        /// <returns></returns>
        Task<bool> ChannelHasDesignation(IGuild guild, IChannel channel, ChannelDesignation designation);

        /// <summary>
        /// Sends the given message (and embed) to the <see cref="IMessageChannel"/>s assigned to the given designation, for a given guild.
        /// </summary>
        /// <param name="guild">The <see cref="IGuild"/> to send the message to</param>
        /// <param name="designation">The <see cref="ChannelDesignation"/> of the channels to send the messages to</param>
        /// <param name="content">The text content of the message</param>
        /// <param name="embed">An optional <see cref="Embed"/> to attach to the message</param>
        /// <returns>A <see cref="Task"/> that, when completed, results in a collection of the messages that were sent.</returns>
        Task<IEnumerable<IMessage>> SendToDesignatedChannelsAsync(IGuild guild, ChannelDesignation designation, string content, Embed embed = null);
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
        public async Task AddDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, ChannelDesignation designation)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ChannelDesignationCreate);

            using (var transaction = await DesignatedChannelMappingRepository.BeginCreateTransactionAsync())
            {
                if (await DesignatedChannelMappingRepository.AnyAsync(new DesignatedChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    IsDeleted = false,
                    ChannelDesignation = designation
                }))
                {
                    throw new InvalidOperationException($"{logChannel.Name} in {guild.Name} is already assigned to {designation}");
                }

                await DesignatedChannelMappingRepository.CreateAsync(new DesignatedChannelMappingCreationData()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    CreatedById = AuthorizationService.CurrentUserId.Value,
                    ChannelDesignation = designation
                });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task RemoveDesignatedChannelAsync(IGuild guild, IMessageChannel logChannel, ChannelDesignation designation)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ChannelDesignationDelete);

            using (var transaction = await DesignatedChannelMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await DesignatedChannelMappingRepository.DeleteAsync(new DesignatedChannelMappingSearchCriteria()
                {
                    GuildId = guild.Id,
                    ChannelId = logChannel.Id,
                    IsDeleted = false,
                    ChannelDesignation = designation
                }, AuthorizationService.CurrentUserId.Value);

                if (deletedCount == 0)
                    throw new InvalidOperationException($"{logChannel.Name} in {guild.Name} is not assigned to {designation}");

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ulong>> GetDesignatedChannelIds(ulong guildId, ChannelDesignation designation)
        {
            var foundChannels = await DesignatedChannelMappingRepository.SearchBriefsAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                IsDeleted = false,
                ChannelDesignation = designation
            });

            return foundChannels.Select(d => d.ChannelId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IMessageChannel>> GetDesignatedChannels(IGuild guild, ChannelDesignation designation)
        {
            var foundChannelIds = await GetDesignatedChannelIds(guild.Id, designation);

            if (!foundChannelIds.Any())
                throw new InvalidOperationException($"{guild.Name} has no channels assigned to {designation}");

            var guildChannels = await Task.WhenAll(foundChannelIds.Select(d => guild.GetChannelAsync(d)));
            return guildChannels.OfType<IMessageChannel>();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IMessage>> SendToDesignatedChannelsAsync(IGuild guild, ChannelDesignation designation, string text, Embed embed = null)
        {
            var channels = await GetDesignatedChannels(guild, designation);

            if (!channels.Any())
            {
                Log.Warning("Warning: Tried to send to channels assigned to designation {designation}, but none were assigned.", new { designation });
            }

            var messages = await Task.WhenAll(channels.Select(channel => channel.SendMessageAsync(text, false, embed)));
            return messages.OfType<IMessage>();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DesignatedChannelMappingBrief>> GetDesignatedChannels(ulong guildId)
        {
            var foundChannels = await DesignatedChannelMappingRepository.SearchBriefsAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guildId,
                IsDeleted = false
            });

            return foundChannels;
        }

        /// <inheritdoc />
        public async Task<bool> ChannelHasDesignation(IGuild guild, IChannel channel, ChannelDesignation designation)
        {
            var foundChannels = await DesignatedChannelMappingRepository.SearchBriefsAsync(new DesignatedChannelMappingSearchCriteria()
            {
                GuildId = guild.Id,
                ChannelDesignation = designation,
                ChannelId = channel.Id,
                IsDeleted = false
            });

            return foundChannels.Any();
        }
    }
}
