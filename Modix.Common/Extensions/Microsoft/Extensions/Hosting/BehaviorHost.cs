using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Acts as the host for all registered <see cref="IBehavior"/>s within the application.
    /// <see cref="IBehavior"/>s are started when the host starts, and stopped when the host stops.
    /// Starting and stopping routines for <see cref="IBehavior"/>s are executed in parallel.
    /// </summary>
    [ServiceBinding(ServiceLifetime.Transient)]
    public class BehaviorHost
        : IHostedService
    {
        public BehaviorHost(
            IEnumerable<IBehavior> behaviors,
            ILogger<BehaviorHost> logger)
        {
            _behaviors = behaviors;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            HostingLogMessages.BehaviorsStarting(_logger);

            await Task.WhenAll(_behaviors
                .Select(async behavior =>
                {
                    using var logScope = HostingLogMessages.BeginBehaviorScope(_logger, behavior);
                    HostingLogMessages.BehaviorStarting(_logger);
                    await behavior.StartAsync(cancellationToken);
                    HostingLogMessages.BehaviorStarted(_logger);
                }));

            HostingLogMessages.BehaviorsStarted(_logger);
        }

        /// <inheritdoc/>
        public async Task StopAsync(
            CancellationToken cancellationToken)
        {
            HostingLogMessages.BehaviorsStopping(_logger);

            await Task.WhenAll(_behaviors
                .Select(async behavior =>
                {
                    using var logScope = HostingLogMessages.BeginBehaviorScope(_logger, behavior);
                    HostingLogMessages.BehaviorStopping(_logger);
                    await behavior.StopAsync(cancellationToken);
                    HostingLogMessages.BehaviorStopped(_logger);
                }));

            HostingLogMessages.BehaviorsStopped(_logger);
        }

        private readonly IEnumerable<IBehavior> _behaviors;
        private readonly ILogger _logger;
    }
}
