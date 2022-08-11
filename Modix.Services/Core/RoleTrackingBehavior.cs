#nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class RoleTrackingBehavior
        : INotificationHandler<JoinedGuildNotification>,
            INotificationHandler<ReadyNotification>,
            INotificationHandler<RoleCreatedNotification>,
            INotificationHandler<RoleUpdatedNotification>
    {
        public RoleTrackingBehavior(
            DiscordSocketClient discordSocketClient,
            IRoleService roleService)
        {
            _discordSocketClient = discordSocketClient;
            _roleService = roleService;
        }

        public async Task HandleNotificationAsync(
            JoinedGuildNotification notification,
            CancellationToken cancellationToken)
        {
            foreach (var role in notification.Guild.Roles)
                await _roleService.TrackRoleAsync(role, cancellationToken);
        }

        public async Task HandleNotificationAsync(
            ReadyNotification notification,
            CancellationToken cancellationToken)
        {
            foreach (var role in _discordSocketClient.Guilds.SelectMany(guild => guild.Roles))
                await _roleService.TrackRoleAsync(role, cancellationToken);
        }

        public Task HandleNotificationAsync(
                RoleCreatedNotification notification,
                CancellationToken cancellationToken)
            => _roleService.TrackRoleAsync(notification.Role, cancellationToken);

        public Task HandleNotificationAsync(
                RoleUpdatedNotification notification,
                CancellationToken cancellationToken)
            => _roleService.TrackRoleAsync(notification.NewRole, cancellationToken);

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IRoleService _roleService;
    }
}
