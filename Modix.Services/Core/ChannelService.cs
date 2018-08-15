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
        /// Updates information about the given channel within the channel tracking..
        /// </summary>
        /// <param name="channel">The channel whose info is to be tracked.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackChannelAsync(IGuildChannel channel);
    }

    /// <inheritdoc />
    public class ChannelService : IChannelService
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        public ChannelService(IDiscordClient discordClient, IGuildChannelRepository guildChannelRepository)
        {
            DiscordClient = discordClient;
            GuildChannelRepository = guildChannelRepository;
        }

        /// <inheritdoc />
        public async Task TrackChannelAsync(IGuildChannel channel)
        {
            using (var transaction = await GuildChannelRepository.BeginCreateTransactionAsync())
            {
                if (!(await GuildChannelRepository.TryUpdateAsync(channel.Id, data =>
                {
                    data.Name = channel.Name;
                })))
                {
                    await GuildChannelRepository.CreateAsync(new GuildChannelCreationData()
                    {
                        ChannelId = channel.Id,
                        GuildId = channel.GuildId,
                        Name = channel.Name
                    });
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IGuildChannelRepository"/> to be used to interact with channel data within a datastore.
        /// </summary>
        internal protected IGuildChannelRepository GuildChannelRepository { get; }
    }
}
