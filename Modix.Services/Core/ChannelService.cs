using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
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
