using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="BaseSocketClient.AuditLogCreated"/> is raised.
    /// </summary>
    public class AuditLogCreatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="AuditLogCreatedNotification"/> from the given values.
        /// </summary>
        /// <param name="entry">The value to use for the entry.</param>
        /// <param name="guild">The value to use for the guild.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="entry"/> and <paramref name="guild"/>.</exception>
        public AuditLogCreatedNotification(SocketAuditLogEntry entry, SocketGuild guild)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Guild = guild ?? throw new ArgumentNullException(nameof(guild));
        }

        /// <summary>
        /// The audit log entry that was created.
        /// </summary>
        public SocketAuditLogEntry Entry { get; }

        /// <summary>
        /// The guild in which the audit log entry was created.
        /// </summary>
        public SocketGuild Guild { get; }
    }
}
