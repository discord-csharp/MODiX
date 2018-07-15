using System;
using System.Threading.Tasks;
using Discord;

using Modix.Services.Authentication;

namespace Modix.Services.Core
{
    /// <inheritdoc />
    public class GuildService : IGuildService
    {
        /// <summary>
        /// Constructs a new <see cref="GuildService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="authenticationService">The value to use for <see cref="AuthenticationService"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for all parameters.</exception>
        public GuildService(IDiscordClient discordClient, IAuthenticationService authenticationService)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            AuthenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        /// <inheritdoc />
        public Task<IGuild> GetCurrentGuildAsync()
            => (AuthenticationService.CurrentGuildId == null)
                ? Task.FromResult<IGuild>(null)
                : GetGuildAsync(AuthenticationService.CurrentGuildId.Value);

        /// <inheritdoc />
        public Task<IGuild> GetGuildAsync(ulong guildId)
            => DiscordClient.GetGuildAsync(guildId);

        /// <summary>
        /// A <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthenticationService"/> to be used to interact with the current authenticated user.
        /// </summary>
        internal protected IAuthenticationService AuthenticationService { get; }

    }
}
