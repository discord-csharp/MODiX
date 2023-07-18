#nullable enable

using System;

using Microsoft.Extensions.Logging;

namespace Modix.Services.Core
{
    internal static class RolesLogMessages
    {
        public enum EventType
        {
            RoleTracking            = 0x0001,
            RoleTracked             = 0x0002,
            RoleUpdating            = 0x0003,
            RoleUpdated             = 0x0004,
            RoleUpdateFailed        = 0x0005,
            RoleCreating            = 0x0006,
            RoleCreated             = 0x0007,
            TransactionBeginning    = 0x0008,
            TransactionCommitting    = 0x0009
        }

        public static IDisposable? BeginRoleScope(
                ILogger logger,
                ulong guildId,
                ulong roleId)
            => _beginRoleScope.Invoke(
                logger,
                guildId,
                roleId);
        private static readonly Func<ILogger, ulong, ulong, IDisposable?> _beginRoleScope
            = LoggerMessage.DefineScope<ulong, ulong>(
                "GuildId: {GuildId}\r\n\tRoleId: {RoleId}");

        public static void RoleCreated(
                ILogger logger)
            => _roleCreated.Invoke(
                logger);
        private static readonly Action<ILogger> _roleCreated
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleCreated.ToEventId(),
                    "Database record created for role")
                .WithoutException();

        public static void RoleCreating(
                ILogger logger)
            => _roleCreating.Invoke(
                logger);
        private static readonly Action<ILogger> _roleCreating
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleCreating.ToEventId(),
                    "Creating database record for role")
                .WithoutException();

        public static void RoleTracked(
                ILogger logger)
            => _roleTracked.Invoke(
                logger);
        private static readonly Action<ILogger> _roleTracked
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleTracked.ToEventId(),
                    "Role tracked within database")
                .WithoutException();

        public static void RoleTracking(
                ILogger logger)
            => _roleTracking.Invoke(
                logger);
        private static readonly Action<ILogger> _roleTracking
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleTracking.ToEventId(),
                    "Tracking role within database")
                .WithoutException();

        public static void RoleUpdated(
                ILogger logger)
            => _roleUpdated.Invoke(
                logger);
        private static readonly Action<ILogger> _roleUpdated
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleUpdated.ToEventId(),
                    "Database record updated for role")
                .WithoutException();

        public static void RoleUpdateFailed(
                ILogger logger)
            => _roleUpdateFailed.Invoke(
                logger);
        private static readonly Action<ILogger> _roleUpdateFailed
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleUpdateFailed.ToEventId(),
                    "Database record for role not found")
                .WithoutException();

        public static void RoleUpdating(
                ILogger logger)
            => _roleUpdating.Invoke(
                logger);
        private static readonly Action<ILogger> _roleUpdating
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.RoleUpdating.ToEventId(),
                    "Updating database record for role")
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

        public static void TransactionCommitting(
                ILogger logger)
            => _transactionCommitting.Invoke(
                logger);
        private static readonly Action<ILogger> _transactionCommitting
            = LoggerMessage.Define(
                    LogLevel.Debug,
                    EventType.TransactionCommitting.ToEventId(),
                    "Committing database transaction")
                .WithoutException();
    }
}
