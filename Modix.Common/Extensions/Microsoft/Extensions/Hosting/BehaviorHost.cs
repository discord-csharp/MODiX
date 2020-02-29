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
                    HostingLogMessages.BehaviorStarting(_logger, behavior);
                    await behavior.StartAsync(cancellationToken);
                    HostingLogMessages.BehaviorStarted(_logger, behavior);
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
                    HostingLogMessages.BehaviorStopping(_logger, behavior);
                    await behavior.StopAsync(cancellationToken);
                    HostingLogMessages.BehaviorStopped(_logger, behavior);
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
