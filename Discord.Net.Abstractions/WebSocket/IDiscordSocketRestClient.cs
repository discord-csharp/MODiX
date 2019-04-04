using System.Threading.Tasks;

using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="DiscordSocketRestClient" />
    public interface IDiscordSocketRestClient : IDiscordRestClient { }

    public class DiscordSocketRestClientAbstraction : DiscordRestClientAbstraction, IDiscordSocketRestClient
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordSocketRestClientAbstraction"/> around an existing <see cref="WebSocket.DiscordSocketRestClient"/>.
        /// </summary>
        /// <param name="discordRestClient">The value to use for <see cref="WebSocket.DiscordSocketRestClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordRestClient"/>.</exception>
        public DiscordSocketRestClientAbstraction(DiscordSocketRestClient discordSocketRestClient)
            : base(discordSocketRestClient) { }

        new public Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
            => DiscordSocketRestClient.LoginAsync(tokenType, token, validateToken);

        new public Task LogoutAsync()
            => DiscordSocketRestClient.LogoutAsync();

        /// <summary>
        /// The existing <see cref="WebSocket.DiscordSocketRestClient"/> being abstracted.
        /// </summary>
        protected DiscordSocketRestClient DiscordSocketRestClient
            => BaseDiscordClient as DiscordSocketRestClient;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="DiscordSocketRestClient"/> objects.
    /// </summary>
    public static class DiscordSocketRestClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="DiscordSocketRestClient"/> to an abstracted <see cref="IDiscordSocketRestClient"/> value.
        /// </summary>
        /// <param name="discordSocketRestClient">The existing <see cref="DiscordSocketRestClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordSocketRestClient"/>.</exception>
        /// <returns>An <see cref="IDiscordSocketRestClient"/> that abstracts <paramref name="discordSocketRestClient"/>.</returns>
        public static IDiscordSocketRestClient Abstract(this DiscordSocketRestClient discordRestClient)
            => new DiscordSocketRestClientAbstraction(discordRestClient);
    }
}
