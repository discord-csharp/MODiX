using System;

using Microsoft.Extensions.Logging;

using Modix.Common;

namespace Microsoft.Extensions.Hosting
{
    internal static class HostingLogMessages
    {
        public enum EventType
        {
            BehaviorsStarting   = CommonLogEventType.Hosting + 0x0001,
            BehaviorsStarted    = CommonLogEventType.Hosting + 0x0002,
            BehaviorsStopping   = CommonLogEventType.Hosting + 0x0003,
            BehaviorsStopped    = CommonLogEventType.Hosting + 0x0004,
            BehaviorStarting    = CommonLogEventType.Hosting + 0x0005,
            BehaviorStarted     = CommonLogEventType.Hosting + 0x0006,
            BehaviorStopping    = CommonLogEventType.Hosting + 0x0007,
            BehaviorStopped     = CommonLogEventType.Hosting + 0x0008
        }

        public static IDisposable? BeginBehaviorScope(
                ILogger logger,
                IBehavior behavior)
            => _beginBehaviorScope.Invoke(
                logger,
                behavior);
        private static readonly Func<ILogger, IBehavior, IDisposable?> _beginBehaviorScope
            = LoggerMessage.DefineScope<IBehavior>(
                "Behavior: {Behavior}");

        public static void BehaviorsStarting(
                ILogger logger)
            => _behaviorsStarting.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorsStarting
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.BehaviorsStarting.ToEventId(),
                    $"Starting {nameof(IBehavior)}'s")
                .WithoutException();

        public static void BehaviorsStarted(
                ILogger logger)
            => _behaviorsStarted.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorsStarted
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.BehaviorsStarted.ToEventId(),
                    $"All {nameof(IBehavior)}'s started")
                .WithoutException();

        public static void BehaviorsStopping(
                ILogger logger)
            => _behaviorsStoping.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorsStoping
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.BehaviorsStopping.ToEventId(),
                    $"Stoping {nameof(IBehavior)}'s")
                .WithoutException();

        public static void BehaviorsStopped(
                ILogger logger)
            => _behaviorsStoped.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorsStoped
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.BehaviorsStopped.ToEventId(),
                    $"All {nameof(IBehavior)}'s stoped")
                .WithoutException();

        public static void BehaviorStarting(
                ILogger logger)
            => _behaviorStarting.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorStarting
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.BehaviorStarting.ToEventId(),
                    $"Starting {nameof(IBehavior)}")
                .WithoutException();

        public static void BehaviorStarted(
                    ILogger logger)
                => _behaviorStarted.Invoke(
                    logger);
        private static readonly Action<ILogger> _behaviorStarted
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.BehaviorStarted.ToEventId(),
                    $"{nameof(IBehavior)} started")
                .WithoutException();

        public static void BehaviorStopping(
                ILogger logger)
            => _behaviorStoping.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorStoping
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.BehaviorStopping.ToEventId(),
                    $"Stoping {nameof(IBehavior)}")
                .WithoutException();

        public static void BehaviorStopped(
                ILogger logger)
            => _behaviorStoped.Invoke(
                logger);
        private static readonly Action<ILogger> _behaviorStoped
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.BehaviorStopped.ToEventId(),
                    $"{nameof(IBehavior)} stoped")
                .WithoutException();
    }
}
