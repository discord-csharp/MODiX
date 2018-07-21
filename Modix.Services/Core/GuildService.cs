using System;
using System.Threading.Tasks;
using Discord;

namespace Modix.Services.Core
{
    /// <inheritdoc />
    public class GuildService : IGuildService
    {
        /// <summary>
        /// Constructs a new <see cref="GuildService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for all parameters.</exception>
        public GuildService(IDiscordClient discordClient)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }

        /// <inheritdoc />
        public Task<IGuild> GetGuildAsync(ulong guildId)
            => DiscordClient.GetGuildAsync(guildId);

        /// <summary>
        /// A <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }
    }
}
