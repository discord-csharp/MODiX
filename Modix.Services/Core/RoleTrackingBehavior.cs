using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Core
{
    /// <summary>
    /// Implements a behavior for keeping the role data within the local datastore synchronized with the Discord API.
    /// </summary>
    public class RoleTrackingBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="RoleTrackingBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        public RoleTrackingBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.Ready += OnReady;
            DiscordClient.JoinedGuild += OnJoinedGuild;
            DiscordClient.RoleCreated += OnRoleCreated;
            DiscordClient.RoleUpdated += OnRoleUpdated;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.Ready -= OnReady;
            DiscordClient.JoinedGuild -= OnJoinedGuild;
            DiscordClient.RoleCreated -= OnRoleCreated;
            DiscordClient.RoleUpdated -= OnRoleUpdated;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsRunning)
                OnStoppedAsync();

            base.Dispose(disposeManaged);
        }

        /// <summary>
        /// A <see cref="DiscordSocketClient"/> to be used for interacting with the Discord API.
        /// </summary>
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnReady()
            => SelfExecuteRequest<IRoleService>(async roleService =>
            {
                foreach (var role in DiscordClient.Guilds.SelectMany(guild => guild.Roles))
                    await roleService.TrackRoleAsync(role);
            });

        private Task OnJoinedGuild(IGuild guild)
            => SelfExecuteRequest<IRoleService>(async roleService =>
            {
                foreach (var role in guild.Roles)
                    await roleService.TrackRoleAsync(role);
            });

        private Task OnRoleCreated(IRole role)
            => SelfExecuteRequest<IRoleService>(roleService => roleService.TrackRoleAsync(role));

        private Task OnRoleUpdated(IRole oldRole, IRole newRole)
            => SelfExecuteRequest<IRoleService>(roleService => roleService.TrackRoleAsync(newRole));
    }
}
