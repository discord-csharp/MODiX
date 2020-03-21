using System;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionLogMessages
    {
        public static void ServiceScopeCreated(
                ILogger logger,
                IServiceScope serviceScope)
            => _serviceScopeCreated.Invoke(
                logger,
                serviceScope);
        private static readonly Action<ILogger, IServiceScope> _serviceScopeCreated
            = LoggerMessage.Define<IServiceScope>(
                    LogLevel.Debug,
                    new EventId(4001, nameof(ServiceScopeCreated)),
                    $"{nameof(IServiceScope)} created: {{ServiceScope}}")
                .WithoutException();
    }
}
