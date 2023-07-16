#nullable enable

using System;

using Microsoft.Extensions.Logging;

namespace Modix.Services.Core
{
    public static class MessageLogMessages
    {
        public enum EventType
        {
            MessageReceivedHandling = ServicesLogEventType.MessageTracking + 0x0001,
            MessageReceivedHandled  = ServicesLogEventType.MessageTracking + 0x0002,
            MessageDeletedHandling  = ServicesLogEventType.MessageTracking + 0x0003,
            MessageDeletedHandled   = ServicesLogEventType.MessageTracking + 0x0004,
            IgnoringCommandMessage  = ServicesLogEventType.MessageTracking + 0x0005,
            IgnoringNonGuildMessage = ServicesLogEventType.MessageTracking + 0x0006,
            IgnoringNonHumanMessage = ServicesLogEventType.MessageTracking + 0x0007,
            MessageRecordCreating   = ServicesLogEventType.MessageTracking + 0x0008,
            MessageRecordCreated    = ServicesLogEventType.MessageTracking + 0x0009,
            MessageRecordDeleting   = ServicesLogEventType.MessageTracking + 0x000A,
            MessageRecordDeleted    = ServicesLogEventType.MessageTracking + 0x000B,
            TransactionBeginning    = ServicesLogEventType.MessageTracking + 0x000C,
            TransactionCommitted    = ServicesLogEventType.MessageTracking + 0x000D
        }

        public static IDisposable? BeginMessageNotificationScope(
                ILogger logger,
                ulong? guildId,
                ulong channelId,
                ulong messageId)
            => _beginMessageNotificationScope.Invoke(
                logger,
                guildId,
                channelId,
                messageId);
        private static readonly Func<ILogger, ulong?, ulong, ulong, IDisposable?> _beginMessageNotificationScope
            = LoggerMessage.DefineScope<ulong?, ulong, ulong>(
                "GuildId: {GuildId}\r\n\tChannelId: {ChannelId}\r\n\tMessageId: {MessageId}");

        public static void IgnoringCommandMessage(
                ILogger logger)
            => _ignoringCommandMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringCommandMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringCommandMessage.ToEventId(),
                    "Ignoring Message: Identified as a Bot Command")
                .WithoutException();

        public static void IgnoringNonGuildMessage(
                ILogger logger)
            => _ignoringNonGuildMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringNonGuildMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringNonGuildMessage.ToEventId(),
                    "Ignoring Message: Does not belong to a Guild")
                .WithoutException();

        public static void IgnoringNonHumanMessage(
                ILogger logger)
            => _ignoringNonHumanMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringNonHumanMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringNonHumanMessage.ToEventId(),
                    "Ignoring Message: Was not sent by a human")
                .WithoutException();

        public static void MessageDeletedHandled(
                ILogger logger)
            => _messageDeletedHandled.Invoke(
                logger);
        private static readonly Action<ILogger> _messageDeletedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageDeletedHandled.ToEventId(),
                    "Deleted message handled")
                .WithoutException();

        public static void MessageDeletedHandling(
                ILogger logger)
            => _messageDeletedHandling.Invoke(
                logger);
        private static readonly Action<ILogger> _messageDeletedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageDeletedHandling.ToEventId(),
                    "Handling deleted message")
                .WithoutException();

        public static void MessageReceivedHandled(
                ILogger logger)
            => _messageReceivedHandled.Invoke(
                logger);
        private static readonly Action<ILogger> _messageReceivedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageReceivedHandled.ToEventId(),
                    "Received message handled")
                .WithoutException();

        public static void MessageReceivedHandling(
                ILogger logger)
            => _messageReceivedHandling.Invoke(
                logger);
        private static readonly Action<ILogger> _messageReceivedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageReceivedHandling.ToEventId(),
                    "Handling received message")
                .WithoutException();

        public static void MessageRecordCreated(
                ILogger logger)
            => _messageRecordCreated.Invoke(
                logger);
        private static readonly Action<ILogger> _messageRecordCreated
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.MessageRecordCreated.ToEventId(),
                    "Database record created for message")
                .WithoutException();

        public static void MessageRecordCreating(
                ILogger logger)
            => _messageRecordCreating.Invoke(
                logger);
        private static readonly Action<ILogger> _messageRecordCreating
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.MessageRecordCreating.ToEventId(),
                    "Creating database record for message")
                .WithoutException();

        public static void MessageRecordDeleted(
                ILogger logger)
            => _messageRecordDeleted.Invoke(
                logger);
        private static readonly Action<ILogger> _messageRecordDeleted
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.MessageRecordDeleted.ToEventId(),
                    "Database record for message deleted")
                .WithoutException();

        public static void MessageRecordDeleting(
                ILogger logger)
            => _messageRecordDeleting.Invoke(
                logger);
        private static readonly Action<ILogger> _messageRecordDeleting
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.MessageRecordDeleting.ToEventId(),
                    "Deleting database record for message")
                .WithoutException();

        public static void TransactionBeginning(
                ILogger logger)
            => _transactionBeginning.Invoke(
                logger);
        private static readonly Action<ILogger> _transactionBeginning
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.TransactionBeginning.ToEventId(),
                    "Beginning database transaction")
                .WithoutException();

        public static void TransactionCommitted(
                ILogger logger)
            => _transactionCommitted.Invoke(
                logger);
        private static readonly Action<ILogger> _transactionCommitted
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.TransactionCommitted.ToEventId(),
                    "Database transaction Committed")
                .WithoutException();
    }
}
