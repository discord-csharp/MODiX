using System.Threading;
using System.Threading.Tasks;

using Discord;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    /// <summary>
    /// Automatically performs configuration necessary for an <see cref="IAuthorizationService"/> to work.
    /// This includes seeding the system with authorization claim mappings for guild administrators, if no claims are present
    /// so that guild administrators have the ability to configure authorization manually.
    /// </summary>
    public class AuthorizationAutoConfigBehavior
        : INotificationHandler<GuildAvailableNotification>,
            INotificationHandler<JoinedGuildNotification>
    {
        /// <summary>
        /// Constructs a new <see cref="AuthorizationAutoConfigBehavior"/> object, with the given dependencies.
        /// </summary>
        public AuthorizationAutoConfigBehavior(
            IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        /// <inheritdoc />
        public Task HandleNotificationAsync(GuildAvailableNotification notification, CancellationToken cancellationToken = default)
            => _authorizationService.AutoConfigureGuildAsync(notification.Guild, cancellationToken);

        /// <inheritdoc />
        public Task HandleNotificationAsync(JoinedGuildNotification notification, CancellationToken cancellationToken = default)
            => ((IGuild)notification.Guild).Available
                ? _authorizationService.AutoConfigureGuildAsync(notification.Guild, cancellationToken)
                : Task.CompletedTask;

        private readonly IAuthorizationService _authorizationService;
    }
}
