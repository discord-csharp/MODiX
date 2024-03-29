using System;

using Microsoft.Extensions.Logging;

namespace Modix.Data
{
    public static class ModixContextLogMessages
    {
        public enum EventType
        {
            ContextMigrating    = DataLogEventType.DbContext + 0x0001,
            ContextMigrated     = DataLogEventType.DbContext + 0x0002
        }

        public static void ContextMigrating(
                ILogger logger)
            => _contextMigrating.Invoke(
                logger);
        private static readonly Action<ILogger> _contextMigrating
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.ContextMigrating.ToEventId(),
                    $"Applying {nameof(ModixContext)} migrations")
                .WithoutException();

        public static void ContextMigrated(
                ILogger logger)
            => _contextMigrated.Invoke(
                logger);
        private static readonly Action<ILogger> _contextMigrated
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.ContextMigrated.ToEventId(),
                    $"{nameof(ModixContext)} migrations applied")
                .WithoutException();
    }
}
