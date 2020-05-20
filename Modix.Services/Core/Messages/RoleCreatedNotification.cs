#nullable enable

using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.RoleCreated"/> is raised.
    /// </summary>
    public class RoleCreatedNotification
        : INotification
    {
        /// <summary>
        /// Constructs a new <see cref="RoleCreatedNotification"/> from the given values.
        /// </summary>
        /// <param name="role">The value to use for <see cref="Role"/>.</param>
        public RoleCreatedNotification(
            ISocketRole role)
        {
            Role = role;
        }

        /// <summary>
        /// The role that was created.
        /// </summary>
        public ISocketRole Role { get; }
    }
}
