#nullable enable

using System;

using Discord;

using Microsoft.Extensions.Logging;

namespace Modix.Services.Moderation
{
    public static class AttachmentBlacklistLogMessages
    {
        public enum EventType
        {
            IgnoringMessageWithNoAttachments    = ModerationLogEventType.AttachmentBlacklist + 0x01,
            IngoringNonGuildMessage             = ModerationLogEventType.AttachmentBlacklist + 0x02,
            IngoringNonHumanMessage             = ModerationLogEventType.AttachmentBlacklist + 0x03,
            IgnoringUnmoderatedChannel          = ModerationLogEventType.AttachmentBlacklist + 0x04,
            SuspiciousAttachmentsSearching      = ModerationLogEventType.AttachmentBlacklist + 0x05,
            SuspiciousAttachmentsNotFound       = ModerationLogEventType.AttachmentBlacklist + 0x06,
            SuspiciousAttachmentsFound          = ModerationLogEventType.AttachmentBlacklist + 0x07,
            ChannelModerationStatusFetching     = ModerationLogEventType.AttachmentBlacklist + 0x08,
            ChannelModerationStatusFetched      = ModerationLogEventType.AttachmentBlacklist + 0x09,
            SelfUserFetched                     = ModerationLogEventType.AttachmentBlacklist + 0x0B,
            SuspiciousMessageDeleting           = ModerationLogEventType.AttachmentBlacklist + 0x0C,
            SuspiciousMessageDeleted            = ModerationLogEventType.AttachmentBlacklist + 0x0D,
            ReplySending                        = ModerationLogEventType.AttachmentBlacklist + 0x0E,
            ReplySent                           = ModerationLogEventType.AttachmentBlacklist + 0x0F
        }

        public static IDisposable BeginMessageScope(
                ILogger logger,
                ulong? guildId,
                ulong channelId,
                ulong authorId,
                ulong messageId)
            => _beginMessageScope.Invoke(
                logger,
                guildId,
                channelId,
                authorId,
                messageId);
        private readonly static Func<ILogger, ulong?, ulong, ulong, ulong, IDisposable> _beginMessageScope
            = LoggerMessageEx.DefineScope<ulong?, ulong, ulong, ulong>(
                "GuildId: {GuildId}\r\n\tChannelId: {ChannelId}\r\n\tAuthorId: {AuthorId}\r\n\tMessageId: {MessageId}");

        public static void ChannelModerationStatusFetched(
                ILogger logger)
            => _channelModerationStatusFetched.Invoke(
                logger);
        private static readonly Action<ILogger> _channelModerationStatusFetched
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ChannelModerationStatusFetched.ToEventId(),
                    "Channel moderation status fetched")
                .WithoutException();

        public static void ChannelModerationStatusFetching(
                ILogger logger)
            => _channelModerationStatusFetching.Invoke(
                logger);
        private static readonly Action<ILogger> _channelModerationStatusFetching
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ChannelModerationStatusFetching.ToEventId(),
                    "Fetching channel moderation status")
                .WithoutException();

        public static void IgnoringMessageWithNoAttachments(
                ILogger logger)
            => _ignoringMessageWithNoAttachments.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringMessageWithNoAttachments
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringMessageWithNoAttachments.ToEventId(),
                    "Ignoring message with no attachments")
                .WithoutException();

        public static void IngoringNonGuildMessage(
                ILogger logger)
            => _ingoringNonGuildMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ingoringNonGuildMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IngoringNonGuildMessage.ToEventId(),
                    "Ignoring non-guild message")
                .WithoutException();

        public static void IngoringNonHumanMessage(
                ILogger logger)
            => _ingoringNonHumanMessage.Invoke(
                logger);
        private static readonly Action<ILogger> _ingoringNonHumanMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IngoringNonHumanMessage.ToEventId(),
                    "Ignoring non-human message")
                .WithoutException();

        public static void IgnoringUnmoderatedChannel(
                ILogger logger)
            => _ignoringUnmoderatedChannel.Invoke(
                logger);
        private static readonly Action<ILogger> _ignoringUnmoderatedChannel
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringUnmoderatedChannel.ToEventId(),
                    "Ignoring message from unmoderated channel")
                .WithoutException();

        public static void ReplySending(
                ILogger logger)
            => _replySending.Invoke(
                logger);
        private static readonly Action<ILogger> _replySending
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ReplySending.ToEventId(),
                    "Sending reply")
                .WithoutException();

        public static void ReplySent(
                ILogger logger)
            => _replySent.Invoke(
                logger);
        private static readonly Action<ILogger> _replySent
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ReplySent.ToEventId(),
                    "Reply sent")
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
                    $"{nameof(ISelfUser)} fetched: SelfUserId: {{SelfUserId}}")
                .WithoutException();

        public static void SuspiciousAttachmentsFound(
                ILogger logger,
                int suspiciousAttachmentCount)
            => _suspiciousAttachmentsFound.Invoke(
                logger,
                suspiciousAttachmentCount);
        private static readonly Action<ILogger, int> _suspiciousAttachmentsFound
            = LoggerMessage.Define<int>(
                    LogLevel.Debug,
                    EventType.SuspiciousAttachmentsFound.ToEventId(),
                    "Suspicious attachments found: {SuspiciousAttachmentCount}")
                .WithoutException();

        public static void SuspiciousAttachmentsNotFound(
                ILogger logger)
            => _suspiciousAttachmentsNotFound.Invoke(
                logger);
        private static readonly Action<ILogger> _suspiciousAttachmentsNotFound
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.SuspiciousAttachmentsNotFound.ToEventId(),
                    "No suspicious attachments found")
                .WithoutException();

        public static void SuspiciousAttachmentsSearching(
                ILogger logger)
            => _suspiciousAttachmentsSearching.Invoke(
                logger);
        private static readonly Action<ILogger> _suspiciousAttachmentsSearching
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.SuspiciousAttachmentsSearching.ToEventId(),
                    "Searching for suspicious attachments")
                .WithoutException();

        public static void SuspiciousMessageDeleted(
                ILogger logger)
            => _suspiciousMessageDeleted.Invoke(
                logger);
        private static readonly Action<ILogger> _suspiciousMessageDeleted
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.SuspiciousMessageDeleted.ToEventId(),
                    "Suspicious message deleted")
                .WithoutException();

        public static void SuspiciousMessageDeleting(
                ILogger logger)
            => _suspiciousMessageDeleting.Invoke(
                logger);
        private static readonly Action<ILogger> _suspiciousMessageDeleting
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.SuspiciousMessageDeleting.ToEventId(),
                    "Deleting suspicious message")
                .WithoutException();
    }
}
