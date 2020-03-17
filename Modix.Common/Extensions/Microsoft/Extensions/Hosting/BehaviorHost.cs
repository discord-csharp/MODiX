using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    public class BehaviorHost
        : IHostedService
    {
        #region Construction

        public BehaviorHost(
            IEnumerable<IBehavior> behaviors,
            ILogger<BehaviorHost> logger)
        {
            _behaviors = behaviors;
            _logger = logger;
        }

        #endregion Construction

        #region IHostedService

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

        #endregion IHostedService

        #region State

        private readonly IEnumerable<IBehavior> _behaviors;
        private readonly ILogger _logger;

        #endregion State
    }
}
