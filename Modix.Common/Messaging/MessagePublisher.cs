using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modix.Common.Messaging
{
    /// <summary>
    /// Describes an object that publishes application-wide notifications.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a notification to be handled by all registered <see cref="INotificationHandler{TNotification}"/> objects.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification to be published.</typeparam>
        /// <param name="notification">The notification data to be published.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that will complete when all handlers have completed handling the notification.</returns>
        Task PublishAsync<TNotification>(
                TNotification notification,
                CancellationToken cancellationToken)
            where TNotification : class;
    }

    /// <inheritdoc />
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class MessagePublisher : IMessagePublisher
    {
        /// <summary>
        /// Constructs a new <see cref="MessagePublisher"/> with the given dependencies.
        /// </summary>
        public MessagePublisher(
            ILogger<MessagePublisher> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken) where TNotification : class
        {
            using var notificationLogScope = MessagingLogMessages.BeginNotificationScope(_logger, notification);

            using var providedLogScope = (notification is ILogScopeProvider logScopeProvider)
                ? logScopeProvider.BeginLogScope(_logger)
                : null;

            MessagingLogMessages.HandlersInvoking(_logger);
            foreach (var handler in _serviceProvider.GetServices<INotificationHandler<TNotification>>())
            {
                using var handlerLogScope = MessagingLogMessages.BeginHandlerScope(_logger, handler);

                MessagingLogMessages.HandlerInvoking(_logger);
                await handler.HandleNotificationAsync(notification, cancellationToken);
                MessagingLogMessages.HandlerInvoked(_logger);
            }
            MessagingLogMessages.HandlersInvoked(_logger);
        }

        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
    }
}
