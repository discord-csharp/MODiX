using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Modix.Common.Messaging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Dispatches notifications to the application during startup and shutdown of the host.
    /// </summary>
    [ServiceBinding(ServiceLifetime.Transient)]
    public class HostLifetimeNotificationBehavior
        : ScopedBehaviorBase
    {
        public HostLifetimeNotificationBehavior(
                ILogger<HostLifetimeNotificationBehavior> logger,
                IServiceScopeFactory serviceScopeFactory)
            : base(
                logger,
                serviceScopeFactory) { }

        /// <inheritdoc/>
        internal protected override async Task OnStartingAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();

            await messagePublisher.PublishAsync(new HostStartingNotification(), cancellationToken);
        }

        /// <inheritdoc/>
        internal protected override async Task OnStoppingAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();

            await messagePublisher.PublishAsync(new HostStoppingNotification(), cancellationToken);
        }
    }
}
