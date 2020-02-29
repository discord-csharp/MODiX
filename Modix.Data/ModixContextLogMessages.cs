using System;

using Microsoft.Extensions.Logging;

namespace Modix.Data
{
    public static class ModixContextLogMessages
    {
        public static void ContextMigrating(
                ILogger logger)
            => _contextMigrating.Invoke(
                logger);
        private static readonly Action<ILogger> _contextMigrating
            = LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(3101, nameof(ContextMigrating)),
                    $"Applying {nameof(ModixContext)} migrations")
                .WithoutException();

        public static void ContextMigrated(
                ILogger logger)
            => _contextMigrated.Invoke(
                logger);
        private static readonly Action<ILogger> _contextMigrated
            = LoggerMessage.Define(
                    LogLevel.Information,
                    new EventId(3102, nameof(ContextMigrated)),
                    $"{nameof(ModixContext)} migrations applied")
                .WithoutException();
    }
}
