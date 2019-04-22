using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Microsoft.Extensions.Caching.Memory;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord channels, within the application.
    /// </summary>
    public interface IChannelService
    {
        /// <summary>
        /// Updates information about the given channel within the channel tracking feature.
        /// </summary>
        /// <param name="channel">The channel whose info is to be tracked.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the returned <see cref="Task"/> before it completes.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackChannelAsync(IGuildChannel channel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retireves information about the given channel.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild in which the channel is located.</param>
        /// <param name="channelId">The unique Discord snowflake ID of the channel.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> that will complete when the operation is complete,
        /// containing information about the requested channel.
        /// </returns>
        ValueTask<ITextChannel> GetChannelAsync(ulong guildId, ulong channelId);
    }

    /// <inheritdoc />
    public class ChannelService : IChannelService
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelService"/> with the given injected dependencies.
        /// </summary>
        public ChannelService(
            IGuildChannelRepository guildChannelRepository,
            IGuildService guildService,
            IMemoryCache cache)
        {
            _guildChannelRepository = guildChannelRepository;
            _guildService = guildService;
            _cache = cache;
        }

        /// <inheritdoc />
        public async Task TrackChannelAsync(IGuildChannel channel, CancellationToken cancellationToken = default)
        {
            using var transaction = await _guildChannelRepository.BeginCreateTransactionAsync(cancellationToken);

            if (channel is ITextChannel textChannel)
            {
                var key = GetKey(channel.GuildId, channel.Id);
                _cache.Set(key, textChannel, TimeSpan.FromDays(7));
            }

            if (!await _guildChannelRepository.TryUpdateAsync(channel.Id, data =>
                {
                    data.Name = channel.Name;
                }, cancellationToken))
            {
                await _guildChannelRepository.CreateAsync(new GuildChannelCreationData()
                {
                    ChannelId = channel.Id,
                    GuildId = channel.GuildId,
                    Name = channel.Name
                }, cancellationToken);
            }

            transaction.Commit();
        }

        public async ValueTask<ITextChannel> GetChannelAsync(ulong guildId, ulong channelId)
        {
            var key = GetKey(guildId, channelId);

            if (!_cache.TryGetValue<ITextChannel>(key, out var channel))
            {
                var guild = await _guildService.GetGuildInformationAsync(guildId, true);
                channel = await guild.Guild.GetTextChannelAsync(channelId);

                _cache.Set(key, channel, TimeSpan.FromDays(7));
            }

            return channel;
        }

        private object GetKey(ulong guildId, ulong channelId)
            => new { Target = "Channel", guildId, channelId };

        private readonly IMemoryCache _cache;
        private readonly IGuildChannelRepository _guildChannelRepository;
        private readonly IGuildService _guildService;
    }
}
