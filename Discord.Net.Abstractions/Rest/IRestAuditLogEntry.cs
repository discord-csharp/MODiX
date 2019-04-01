using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestAuditLogEntry" />
    public interface IRestAuditLogEntry : IAuditLogEntry, ISnowflakeEntity, IEntity<ulong> { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestAuditLogEntry"/>, through the <see cref="IRestAuditLogEntry"/> interface.
    /// </summary>
    public class RestAuditLogEntryAbstraction : IRestAuditLogEntry
    {
        /// <summary>
        /// Constructs a new <see cref="RestAuditLogEntryAbstraction"/> around an existing <see cref="Rest.RestAuditLogEntry"/>.
        /// </summary>
        /// <param name="restAuditLogEntry">The value to use for <see cref="Rest.RestAuditLogEntry"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restAuditLogEntry"/>.</exception>
        public RestAuditLogEntryAbstraction(RestAuditLogEntry restAuditLogEntry)
        {
            RestAuditLogEntry = restAuditLogEntry ?? throw new ArgumentNullException(nameof(restAuditLogEntry));
        }

        /// <inheritdoc />
        public ActionType Action
            => RestAuditLogEntry.Action;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestAuditLogEntry.CreatedAt;

        /// <inheritdoc />
        public IAuditLogData Data
            => RestAuditLogEntry.Data;

        /// <inheritdoc />
        public ulong Id
            => RestAuditLogEntry.Id;

        /// <inheritdoc />
        public string Reason
            => RestAuditLogEntry.Reason;

        /// <inheritdoc />
        public IUser User
            => RestAuditLogEntry.User;

        /// <summary>
        /// The existing <see cref="Rest.RestAuditLogEntry"/> being abstracted.
        /// </summary>
        protected RestAuditLogEntry RestAuditLogEntry { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestAuditLogEntry"/> objects.
    /// </summary>
    public static class RestAuditLogEntryAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestAuditLogEntry"/> to an abstracted <see cref="IRestAuditLogEntry"/> value.
        /// </summary>
        /// <param name="restAuditLogEntry">The existing <see cref="RestAuditLogEntry"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restAuditLogEntry"/>.</exception>
        /// <returns>An <see cref="IRestAuditLogEntry"/> that abstracts <paramref name="restAuditLogEntry"/>.</returns>
        public static IRestAuditLogEntry Abstract(this RestAuditLogEntry restAuditLogEntry)
            => new RestAuditLogEntryAbstraction(restAuditLogEntry);
    }
}
