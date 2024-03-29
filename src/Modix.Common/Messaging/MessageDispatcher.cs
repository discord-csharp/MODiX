using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Modix.Common.Messaging
{
    /// <summary>
    /// Describes an object that dispatches application-wide notifications.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatches a notification to be handled by all registered <see cref="INotificationHandler{TNotification}"/> objects.
        /// on a new logical application thread. I.E. handlers for the notification are executed within a new <see cref="IServiceScope"/>
        /// and synchronized through a new <see cref="Task"/> that will run in parallel to the current one, on the <see cref="ThreadPool"/>.
        /// Note that exceptions thrown by and handler of the notification will be logged, and will not affect the execution of other handlers for the notification.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification to be dispatched.</typeparam>
        /// <param name="notification">The notification data to be dispatched.</param>
        /// <param name="timeout">The amount of time to wait for the dispatch operation to complete, before cancelling it. Defaults to whatever is configured by </param>
        void Dispatch<TNotification>(
                TNotification notification,
                TimeSpan? timeout = default)
            where TNotification : class;
    }

    /// <inheritdoc />
    [ServiceBinding(ServiceLifetime.Singleton)]
    public class MessageDispatcher : IMessageDispatcher
    {
        /// <summary>
        /// Constructs a new <see cref="MessageDispatcher"/> with the given dependencies.
        /// </summary>
        public MessageDispatcher(
            ICancellationTokenSourceFactory cancellationTokenSourceFactory,
            ILogger<MessageDispatcher> logger,
            IOptions<MessagingOptions> options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _cancellationTokenSourceFactory = cancellationTokenSourceFactory;
            _logger = logger;
            _options = options;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc />
        public void Dispatch<TNotification>(
                TNotification notification,
                TimeSpan? timeout = default)
            where TNotification : class
        {
            _ = DispatchAsync(notification);
        }

        // internal for testing
        internal async Task DispatchAsync<TNotification>(
                TNotification notification,
                TimeSpan? timeout = default)
            where TNotification : class
        {
            try
            {
                using var cancellationTokenSource = _cancellationTokenSourceFactory.Create(timeout ?? _options.Value.DispatchTimeout ?? Timeout.InfiniteTimeSpan);
                using var serviceScope = _serviceScopeFactory.CreateScope();
                using var notificationLogScope = MessagingLogMessages.BeginNotificationScope(_logger, notification);

                using var providedLogScope = (notification is ILogScopeProvider logScopeProvider)
                    ? logScopeProvider.BeginLogScope(_logger)
                    : null;

                MessagingLogMessages.HandlersInvoking(_logger);
                foreach (var handler in serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>())
                {
                    using var handlerLogScope = MessagingLogMessages.BeginHandlerScope(_logger, handler);

                    try
                    {
                        MessagingLogMessages.HandlerInvoking(_logger);
                        await handler.HandleNotificationAsync(notification, cancellationTokenSource.Token);
                        MessagingLogMessages.HandlerInvoked(_logger);
                    }
                    catch (Exception ex)
                    {
                        MessagingLogMessages.HandlerFailed(_logger, ex);
                    }
                }
                MessagingLogMessages.HandlersInvoked(_logger);
            }
            catch (Exception ex)
            {
                MessagingLogMessages.DispatchFailed(_logger, ex);
            }
        }

        private readonly ICancellationTokenSourceFactory _cancellationTokenSourceFactory;
        private readonly ILogger _logger;
        private readonly IOptions<MessagingOptions> _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
    }
}
