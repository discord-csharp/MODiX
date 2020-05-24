#nullable enable

using System;

using Microsoft.Extensions.Logging;

namespace Modix.Services.UserMetrics
{
    internal static class UserMetricsLogMessages
    {
        public enum EventType
        {
            GuildAvailableHandling          = ServicesLogEventType.UserMetrics + 0x0001,
            GuildAvailableHandled           = ServicesLogEventType.UserMetrics + 0x0002,
            MessageReceivedHandling         = ServicesLogEventType.UserMetrics + 0x0003,
            MessageReceivedHandled          = ServicesLogEventType.UserMetrics + 0x0004,
            UserBannedHandling              = ServicesLogEventType.UserMetrics + 0x0005,
            UserBannedHandled               = ServicesLogEventType.UserMetrics + 0x0006,
            UserJoinedHandling              = ServicesLogEventType.UserMetrics + 0x0007,
            UserJoinedHandled               = ServicesLogEventType.UserMetrics + 0x0008,
            UserLeftHandling                = ServicesLogEventType.UserMetrics + 0x0009,
            UserLeftHandled                 = ServicesLogEventType.UserMetrics + 0x000A,
            ChannelParticipationFetching    = ServicesLogEventType.UserMetrics + 0x000B,
            CharacterParticipationFetched   = ServicesLogEventType.UserMetrics + 0x000C,
            ChannelParticipationFetchFailed = ServicesLogEventType.UserMetrics + 0x000D,
            IgnoringNonGuildMessage         = ServicesLogEventType.UserMetrics + 0x000E,
            IgnoringNonHumanMessage         = ServicesLogEventType.UserMetrics + 0x000F,
            UserRankFetching                = ServicesLogEventType.UserMetrics + 0x0010,
            UserRankFetched                 = ServicesLogEventType.UserMetrics + 0x0011,
            UserRankFetchFailed             = ServicesLogEventType.UserMetrics + 0x0012
        }

        public static IDisposable BeginGuildScope(
                ILogger logger,
                ulong guildId)
            => _beginGuildScope.Invoke(
                logger,
                guildId);
        private static readonly Func<ILogger, ulong, IDisposable> _beginGuildScope
            = LoggerMessage.DefineScope<ulong>(
                "GuildId: {GuildId}");

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
        private static readonly Func<ILogger, ulong?, ulong, ulong, ulong, IDisposable> _beginMessageScope
            = LoggerMessageEx.DefineScope<ulong?, ulong, ulong, ulong>(
                "GuildId: {GuildId}\r\n\tChannelId: {ChannelId}\r\n\tAuthorId: {AuthorId}\r\n\tMessageId: {MessageId}");

        public static void ChannelParticipationFetched(
                ILogger logger,
                bool isOnTopic)
            => _characterParticipationFetched(
                logger,
                isOnTopic);
        private static readonly Action<ILogger, bool> _characterParticipationFetched
            = LoggerMessage.Define<bool>(
                    LogLevel.Debug,
                    EventType.CharacterParticipationFetched.ToEventId(),
                    "Channel participation designations fetched\r\n\tIsOnTopic: {IsOnTopic}")
                .WithoutException();

        public static void ChannelParticipationFetching(
                ILogger logger)
            => _channelParticipationFetching(
                logger);
        private static readonly Action<ILogger> _channelParticipationFetching
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.ChannelParticipationFetching.ToEventId(),
                    "Fetching channel participation designations")
                .WithoutException();

        public static void ChannelParticipationFetchFailed(
                ILogger logger,
                Exception exception)
            => _channelParticipationFetchFailed(
                logger,
                exception);
        private static readonly Action<ILogger, Exception> _channelParticipationFetchFailed
            = LoggerMessage.Define(
                LogLevel.Error,
                EventType.ChannelParticipationFetchFailed.ToEventId(),
                "Failed to fetch channel participation designations");

        public static void IgnoringNonGuildMessage(
                ILogger logger)
            => _ignoringNonGuildMessage(
                logger);
        private static readonly Action<ILogger> _ignoringNonGuildMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringNonGuildMessage.ToEventId(),
                    "Ignoring non-guild message")
                .WithoutException();

        public static void IgnoringNonHumanMessage(
                ILogger logger)
            => _ignoringNonHumanMessage(
                logger);
        private static readonly Action<ILogger> _ignoringNonHumanMessage
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.IgnoringNonHumanMessage.ToEventId(),
                    "Ignoring non-human message")
                .WithoutException();

        public static void GuildAvailableHandled(
                ILogger logger)
            => _guildAvailableHandled(
                logger);
        private static readonly Action<ILogger> _guildAvailableHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.GuildAvailableHandled.ToEventId(),
                    "GuildAvailable notification handled")
                .WithoutException();

        public static void GuildAvailableHandling(
                ILogger logger)
            => _guildAvailableHandling(
                logger);
        private static readonly Action<ILogger> _guildAvailableHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.GuildAvailableHandling.ToEventId(),
                    "Handling GuildAvailable notification")
                .WithoutException();

        public static void MessageReceivedHandled(
                ILogger logger)
            => _messageReceivedHandled(
                logger);
        private static readonly Action<ILogger> _messageReceivedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageReceivedHandled.ToEventId(),
                    "MessageReceived notification handled")
                .WithoutException();

        public static void MessageReceivedHandling(
                ILogger logger)
            => _messageReceivedHandling(
                logger);
        private static readonly Action<ILogger> _messageReceivedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.MessageReceivedHandling.ToEventId(),
                    "Handling MessageReceived notification")
                .WithoutException();

        public static void UserBannedHandled(
                ILogger logger)
            => _userBannedHandled(
                logger);
        private static readonly Action<ILogger> _userBannedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserBannedHandled.ToEventId(),
                    "UserBanned notification handled")
                .WithoutException();

        public static void UserBannedHandling(
                ILogger logger)
            => _userBannedHandling(
                logger);
        private static readonly Action<ILogger> _userBannedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserBannedHandling.ToEventId(),
                    "Handling UserBanned notification")
                .WithoutException();

        public static void UserJoinedHandled(
                ILogger logger)
            => _userJoinedHandled(
                logger);
        private static readonly Action<ILogger> _userJoinedHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserJoinedHandled.ToEventId(),
                    "UserJoined notification handled")
                .WithoutException();

        public static void UserJoinedHandling(
                ILogger logger)
            => _userJoinedHandling(
                logger);
        private static readonly Action<ILogger> _userJoinedHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserJoinedHandling.ToEventId(),
                    "Handling UserJoined notification")
                .WithoutException();

        public static void UserLeftHandled(
                ILogger logger)
            => _userLeftHandled(
                logger);
        private static readonly Action<ILogger> _userLeftHandled
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserLeftHandled.ToEventId(),
                    "UserLeft notification handled")
                .WithoutException();

        public static void UserLeftHandling(
                ILogger logger)
            => _userLeftHandling(
                logger);
        private static readonly Action<ILogger> _userLeftHandling
            = LoggerMessage.Define(
                    LogLevel.Information,
                    EventType.UserLeftHandling.ToEventId(),
                    "Handling UserLeft notification")
                .WithoutException();

        public static void UserRankFetched(
                ILogger logger,
                bool isRanked)
            => _userRankFetched(
                logger,
                isRanked);
        private static readonly Action<ILogger, bool> _userRankFetched
            = LoggerMessage.Define<bool>(
                    LogLevel.Debug,
                    EventType.UserRankFetched.ToEventId(),
                    "User rank information fetched\r\n\tIsRanked: {IsRanked}")
                .WithoutException();

        public static void UserRankFetching(
                ILogger logger)
            => _userRankFetching(
                logger);
        private static readonly Action<ILogger> _userRankFetching
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.UserRankFetching.ToEventId(),
                    "Fetching user rank information")
                .WithoutException();

        public static void UserRankFetchFailed(
                ILogger logger,
                Exception exception)
            => _userRankFetchFailed(
                logger,
                exception);
        private static readonly Action<ILogger, Exception> _userRankFetchFailed
            = LoggerMessage.Define(
                LogLevel.Error,
                EventType.UserRankFetchFailed.ToEventId(),
                "Failed to fetch user rank information");
    }
}
