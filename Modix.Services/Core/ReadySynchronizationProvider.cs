using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides application-wide synchronization for asynchronous operations,
    /// based on the <see cref="IDiscordSocketClient.Ready"/>.
    /// </summary>
    public interface IReadySynchronizationProvider
    {
        /// <summary>
        /// A <see cref="Task"/> that will complete the first time <see cref="IDiscordSocketClient.Ready"/> is raised.
        /// </summary>
        Task WhenReady { get; }
    }

    /// <inheritdoc />
    public class ReadySynchronizationProvider
        : IReadySynchronizationProvider,
            INotificationHandler<ReadyNotification>
    {
        public ReadySynchronizationProvider()
        {
            _whenReadySource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public Task WhenReady
            => _whenReadySource.Task;

        public Task HandleNotificationAsync(ReadyNotification notification, CancellationToken cancellationToken = default)
        {
            _whenReadySource.TrySetResult(null);

            return Task.CompletedTask;
        }

        private readonly TaskCompletionSource<object> _whenReadySource;
    }
}
