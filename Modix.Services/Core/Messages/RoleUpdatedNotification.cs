#nullable enable

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.RoleUpdated"/> is raised.
    /// </summary>
    public class RoleUpdatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="RoleUpdatedNotification"/> from the given values.
        /// </summary>
        /// <param name="oldRole">The value to use for <see cref="OldRole"/>.</param>
        /// <param name="newRole">The value to use for <see cref="NewRole"/>.</param>
        public RoleUpdatedNotification(
            ISocketRole oldRole,
            ISocketRole newRole)
        {
            OldRole = oldRole;
            NewRole = newRole;
        }

        /// <summary>
        /// The state of the role that was updated, prior to the update.
        /// </summary>
        public ISocketRole OldRole { get; }

        /// <summary>
        /// The state of the role that was updated, after the update.
        /// </summary>
        public ISocketRole NewRole { get; }
    }
}
