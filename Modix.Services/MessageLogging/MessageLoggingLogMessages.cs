#nullable enable

using System;

using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace Modix.Services.MessageLogging
{
    public static class MessageLoggingLogMessages
    {
        public enum EventType
        {
            MessageDeletedHandling      = ServicesLogEventType.MessageLogging + 0x0001,
            MessageDeletedHandled       = ServicesLogEventType.MessageLogging + 0x0002,
            MessageUpdatedHandling      = ServicesLogEventType.MessageLogging + 0x0003,
            MessageUpdatedHandled       = ServicesLogEventType.MessageLogging + 0x0004,
            IgnoringNonGuildMessage     = ServicesLogEventType.MessageLogging + 0x0005,
            IgnoringUnchangedMessage    = ServicesLogEventType.MessageLogging + 0x0006,
            IgnoringUnmoderatedChannel  = ServicesLogEventType.MessageLogging + 0x0007,
            ModeratedChannelIdentified  = ServicesLogEventType.MessageLogging + 0x0008,
            SelfUserFetched             = ServicesLogEventType.MessageLogging + 0x0009,
            IgnoringSelfAuthoredMessage = ServicesLogEventType.MessageLogging + 0x000A,
            MessageLogChannelsFetched   = ServicesLogEventType.MessageLogging + 0x000B,
            MessageLogChannelsNotFound  = ServicesLogEventType.MessageLogging + 0x000C,
            MessageLogMessageSending    = ServicesLogEventType.MessageLogging + 0x000D,
            MessageLogMessageSent       = ServicesLogEventType.MessageLogging + 0x000E
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

        public static void IgnoringSelfAuthoredMessage(
                ILogger logger)
            => _ignoringSelfAuthoredMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringSelfAuthoredMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringSelfAuthoredMessage.ToEventId(),
                    "Ignoring Message: Author is Self")
                .WithoutException();

        public static void IgnoringUnchangedMessage(
                ILogger logger)
            => _ignoringUnchangedMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringUnchangedMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringUnchangedMessage.ToEventId(),
                    "Ignoring Message: Content is unchanged")
                .WithoutException();

        public static void IgnoringUnmoderatedChannel(
                ILogger logger)
            => _ignoringUnmoderatedChannel.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringUnmoderatedChannel
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringUnmoderatedChannel.ToEventId(),
                    "Ignoring Message: Channel is Unmoderated")
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

        public static void MessageUpdatedHandled(
                ILogger logger)
            => _messageUpdatedHandled.Invoke(
                logger);
        private static readonly Action<ILogger> _messageUpdatedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageUpdatedHandled.ToEventId(),
                    "Updated message handled")
                .WithoutException();

        public static void MessageUpdatedHandling(
                ILogger logger)
            => _messageUpdatedHandling.Invoke(
                logger);
        private static readonly Action<ILogger> _messageUpdatedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageUpdatedHandling.ToEventId(),
                    "Handling Updated message")
                .WithoutException();

        public static void MessageLogChannelsFetched(
                ILogger logger,
                int messageLogChannelCount)
            => _messageLogChannelsFetched.Invoke(
                logger,
                messageLogChannelCount);
        private static readonly Action<ILogger, int> _messageLogChannelsFetched
            = LoggerMessage.Define<int>(
                    LogLevel.Debug,
                    EventType.MessageLogChannelsFetched.ToEventId(),
                    "Fetched {MessageLogChannelCount} MessageLog channels")
                .WithoutException();

        public static void MessageLogChannelsNotFound(
                ILogger logger)
            => _messageLogChannelsNotFound.Invoke(
                logger);
        private static readonly Action<ILogger> _messageLogChannelsNotFound
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.MessageLogChannelsNotFound.ToEventId(),
                    "No MessageLog channels found")
                .WithoutException();

        public static void MessageLogMessageSent(
                ILogger logger,
                ulong messageLogChannelId,
                ulong messageLogMessageId)
            => _messageLogMessageSent.Invoke(
                logger,
                messageLogChannelId,
                messageLogMessageId);
        private static readonly Action<ILogger, ulong, ulong> _messageLogMessageSent
            = LoggerMessage.Define<ulong, ulong>(
                    LogLevel.Information,
                    EventType.MessageLogMessageSent.ToEventId(),
                    "MessageLog message sent: MesssageLogChannelId: {MessageLogChannelId}\r\n\tMessageLogMessageId: {MessageLogMessageId}")
                .WithoutException();

        public static void MessageLogMessageSending(
                ILogger logger,
                ulong messageLogChannelId)
            => _messageLogMessageSending.Invoke(
                logger,
                messageLogChannelId);
        private static readonly Action<ILogger, ulong> _messageLogMessageSending
            = LoggerMessage.Define<ulong>(
                    LogLevel.Information,
                    EventType.MessageLogMessageSending.ToEventId(),
                    "Sending MessageLog message: MessageLogChannelId: {MessageLogChannelId}")
                .WithoutException();

        public static void ModeratedChannelIdentified(
                ILogger logger)
            => _moderatedChannelIdentified.Invoke(
                logger);
        private static readonly Action<ILogger> _moderatedChannelIdentified
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ModeratedChannelIdentified.ToEventId(),
                    "Identified message channel as not Unmoderated")
                .WithoutException();

        public static void SelfUserFetched(
                ILogger logger,
                ulong selfUserId)
            => _selfUserFetched.Invoke(
                logger,
                selfUserId);
        private static readonly Action<ILogger, ulong> _selfUserFetched
            = LoggerMessage.Define<ulong>(
                    LogLevel.Debug,
                    EventType.SelfUserFetched.ToEventId(),
                    $"Fetched {nameof(SocketSelfUser)}: {{SelfUserId}}")
                .WithoutException();
    }
}
