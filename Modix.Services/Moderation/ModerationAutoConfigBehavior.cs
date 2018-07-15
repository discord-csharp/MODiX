using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a behavior that automatically performs configuration necessary for an <see cref="IModerationService"/> to work.
    /// </summary>
    public class ModerationAutoConfigBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="ModerationAutoConfigBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordClient"/>.</exception>
        public ModerationAutoConfigBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));

            DiscordClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordClient.ChannelCreated += OnChannelCreated;
            DiscordClient.ChannelUpdated += OnChannelUpdated;
            DiscordClient.LeftGuild += OnLeftGuild;
        }

        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnGuildAvailableAsync(IGuild guild)
            => ExecuteOnModerationServiceAsync(x => x.AutoConfigureGuldAsync(guild));

        private Task OnChannelCreated(IChannel channel)
            => ExecuteOnModerationServiceAsync(x => x.AutoConfigureChannelAsync(channel));

        private Task OnChannelUpdated(IChannel oldChannel, IChannel newChannel)
            => ExecuteOnModerationServiceAsync(x => x.AutoConfigureChannelAsync(newChannel));

        private Task OnLeftGuild(IGuild guild)
            => ExecuteOnModerationServiceAsync(x => x.UnConfigureGuildAsync(guild));

        /// <summary>
        /// Retrieves an <see cref="IModerationService"/> for a new request scope, and executes the given action,
        /// within that scope, and with the retrieved service.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        internal protected Task ExecuteOnModerationServiceAsync(Func<IModerationService, Task> action)
            => ExecuteScopedAsync(serviceProvider =>
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                return action.Invoke(serviceProvider.GetService<IModerationService>());
            });
    }
}
