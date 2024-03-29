using System;

using Microsoft.Extensions.Logging;

namespace Modix.Common.Messaging
{
    internal static class MessagingLogMessages
    {
        public enum EventType
        {
            HandlersInvoking    = CommonLogEventType.Messaging + 0x0100,
            HandlersInvoked     = CommonLogEventType.Messaging + 0x0200,
            HandlerInvoking     = CommonLogEventType.Messaging + 0x0300,
            HandlerInvoked      = CommonLogEventType.Messaging + 0x0400,
            HandlerFailed       = CommonLogEventType.Messaging + 0x0500,
            DispatchFailed      = CommonLogEventType.Messaging + 0x0600
        }

        public static IDisposable? BeginHandlerScope(
                ILogger logger,
                object handler)
            => _beginHandlerScope.Invoke(
                logger,
                handler.GetType().Name);
        private static readonly Func<ILogger, string, IDisposable?> _beginHandlerScope
            = LoggerMessage.DefineScope<string>(
                "NotificationHandlerType: {NotificationHandlerType}");

        public static IDisposable? BeginNotificationScope(
                ILogger logger,
                object notification)
            => _beginNotificationScope.Invoke(
                logger,
                notification.GetType().Name,
                notification.GetHashCode());
        private static readonly Func<ILogger, string, int, IDisposable?> _beginNotificationScope
            = LoggerMessage.DefineScope<string, int>(
                "NotificationType: {NotificationType}\r\n\tNotificationHashCode: {NotificationHashCode}");

        public static void DispatchFailed(
                ILogger logger,
                Exception exception)
            => _dispatchFailed.Invoke(
                logger,
                exception);
        private static readonly Action<ILogger, Exception> _dispatchFailed
            = LoggerMessage.Define(
                LogLevel.Error,
                EventType.DispatchFailed.ToEventId(),
                "An unexpected exception occurred during dispatching of a notification.");

        public static void HandlerFailed(
                ILogger logger,
                Exception exception)
            => _handlerFailed.Invoke(
                logger,
                exception);
        private static readonly Action<ILogger, Exception> _handlerFailed
            = LoggerMessage.Define(
                LogLevel.Error,
                EventType.HandlerFailed.ToEventId(),
                "A notification handler threw an exception, during dispatching of a notification.");

        public static void HandlerInvoked(
                ILogger logger)
            => _handlerInvoked.Invoke(
                logger);
        private static readonly Action<ILogger> _handlerInvoked
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.HandlerInvoked.ToEventId(),
                    "Notification handler invoked")
                .WithoutException();

        public static void HandlerInvoking(
                ILogger logger)
            => _handlerInvoking.Invoke(
                logger);
        private static readonly Action<ILogger> _handlerInvoking
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.HandlerInvoking.ToEventId(),
                    "Invoking notification handler")
                .WithoutException();

        public static void HandlersInvoked(
                ILogger logger)
            => _handlersInvoked.Invoke(
                logger);
        private static readonly Action<ILogger> _handlersInvoked
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.HandlersInvoked.ToEventId(),
                    "Notification handlers invoked")
                .WithoutException();

        public static void HandlersInvoking(
                ILogger logger)
            => _handlersInvoking.Invoke(
                logger);
        private static readonly Action<ILogger> _handlersInvoking
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.HandlersInvoking.ToEventId(),
                    "Invoking notification handlers")
                .WithoutException();
    }
}
