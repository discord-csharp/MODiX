using System.Threading;
using System.Threading.Tasks;

using Discord.WebSocket;

using Nito.AsyncEx;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides application-wide synchronization for access to <see cref="IBaseSocketClient.CurrentUser"/>,
    /// which is only available after <see cref="IDiscordSocketClient.Ready"/> has occurred.
    /// </summary>
    public interface ISelfUserProvider
    {
        Task<ISocketSelfUser> GetSelfUserAsync(CancellationToken cancellationToken = default);
    }

    /// <inheritdoc />
    public class SelfUserProvider
        : ISelfUserProvider
    {
        public SelfUserProvider(
            IDiscordSocketClient discordSocketClient,
            IReadySynchronizationProvider readySynchronizationProvider)
        {
            _discordSocketClient = discordSocketClient;
            _readySynchronizationProvider = readySynchronizationProvider;
        }

        /// <inheritdoc />
        public async Task<ISocketSelfUser> GetSelfUserAsync(CancellationToken cancellationToken = default)
        {
            await _readySynchronizationProvider.WhenReady.WaitAsync(cancellationToken);

            return _discordSocketClient.CurrentUser;
        }

        private readonly IDiscordSocketClient _discordSocketClient;

        private readonly IReadySynchronizationProvider _readySynchronizationProvider;
    }
}
