using System.Threading;
using System.Threading.Tasks;

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
        Task TrackChannelAsync(string channelName, ulong channelId, ulong guildId, ulong? parentChannelId, CancellationToken cancellationToken = default);
    }

    /// <inheritdoc />
    public class ChannelService : IChannelService
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelService"/> with the given injected dependencies.
        /// </summary>
        public ChannelService(
            IGuildChannelRepository guildChannelRepository)
        {
            _guildChannelRepository = guildChannelRepository;
        }

        /// <inheritdoc />
        public async Task TrackChannelAsync(string channelName, ulong channelId, ulong guildId, ulong? parentChannelId, CancellationToken cancellationToken = default)
        {
            using var transaction = await _guildChannelRepository.BeginCreateTransactionAsync(cancellationToken);

            if (!await _guildChannelRepository.TryUpdateAsync(channelId, data =>
            {
                data.Name = channelName;
                data.ParentChannelId = parentChannelId;
            }, cancellationToken))
            {
                await _guildChannelRepository.CreateAsync(new GuildChannelCreationData()
                {
                    ChannelId = channelId,
                    GuildId = guildId,
                    Name = channelName,
                    ParentChannelId = parentChannelId,
                }, cancellationToken);
            }

            transaction.Commit();
        }

        private readonly IGuildChannelRepository _guildChannelRepository;
    }
}
