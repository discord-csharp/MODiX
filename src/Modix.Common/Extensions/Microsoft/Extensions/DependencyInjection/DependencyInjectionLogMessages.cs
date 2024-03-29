using System;

using Microsoft.Extensions.Logging;

using Modix.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionLogMessages
    {
        public enum EventType
        {
            ServiceScopeCreated = CommonLogEventType.DependencyInjection + 0x0001
        }

        public static void ServiceScopeCreated(
                ILogger logger,
                IServiceScope serviceScope)
            => _serviceScopeCreated.Invoke(
                logger,
                serviceScope);
        private static readonly Action<ILogger, IServiceScope> _serviceScopeCreated
            = LoggerMessage.Define<IServiceScope>(
                    LogLevel.Debug,
                    EventType.ServiceScopeCreated.ToEventId(),
                    $"{nameof(IServiceScope)} created: {{ServiceScope}}")
                .WithoutException();
    }
}
