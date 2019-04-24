using System.Threading;
using System.Threading.Tasks;

using Discord;

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
        public async Task TrackChannelAsync(IGuildChannel channel, CancellationToken cancellationToken = default)
        {
            using (var transaction = await _guildChannelRepository.BeginCreateTransactionAsync(cancellationToken))
            {
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
        }

        private readonly IGuildChannelRepository _guildChannelRepository;
    }
}
