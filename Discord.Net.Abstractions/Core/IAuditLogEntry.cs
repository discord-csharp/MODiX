using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IAuditLogEntry"/> objects.
    /// </summary>
    internal static class AuditLogEntryAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IAuditLogEntry"/> to an abstracted <see cref="IAuditLogEntry"/> value.
        /// </summary>
        /// <param name="auditLogEntry">The existing <see cref="IAuditLogEntry"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="auditLogEntry"/>.</exception>
        /// <returns>An <see cref="IAuditLogEntry"/> that abstracts <paramref name="auditLogEntry"/>.</returns>
        public static IAuditLogEntry Abstract(this IAuditLogEntry auditLogEntry)
            => auditLogEntry switch
            {
                null
                    => throw new ArgumentNullException(nameof(auditLogEntry)),
                RestAuditLogEntry restAuditLogEntry
                    => RestAuditLogEntryAbstractionExtensions.Abstract(restAuditLogEntry),
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IAuditLogEntry)} type {auditLogEntry.GetType().Name}")
            };
    }
}
