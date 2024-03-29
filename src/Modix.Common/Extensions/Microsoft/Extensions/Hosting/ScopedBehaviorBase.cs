using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Defines a behavior that performs its startup and shutdown actions within an <see cref="IServiceScope"/>.
    /// </summary>
    public abstract class ScopedBehaviorBase
        : IBehavior
    {
        protected ScopedBehaviorBase(
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc/>
        public virtual async Task StartAsync(
            CancellationToken cancellationToken)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            DependencyInjectionLogMessages.ServiceScopeCreated(_logger, serviceScope);

            await OnStartingAsync(
                serviceScope.ServiceProvider,
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task StopAsync(
            CancellationToken cancellationToken)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            DependencyInjectionLogMessages.ServiceScopeCreated(_logger, serviceScope);

            await OnStoppingAsync(
                serviceScope.ServiceProvider,
                cancellationToken);
        }

        /// <summary>
        /// Allows subclasses to perform startup actions within an <see cref="IServiceScope"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceScope.ServiceProvider"/> to be used to perform startup actions.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the operation early.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation completes.</returns>
        internal protected abstract Task OnStartingAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);

        /// <summary>
        /// Allows subclasses to perform shutdown actions within an <see cref="IServiceScope"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceScope.ServiceProvider"/> to be used to perform shutdown actions.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the operation early.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation completes.</returns>
        internal protected abstract Task OnStoppingAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);

        protected readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
    }
}
