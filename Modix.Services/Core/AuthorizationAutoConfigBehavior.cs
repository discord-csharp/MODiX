﻿using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Core
{
    /// <summary>
    /// Implements a behavior that automatically performs configuration necessary for an <see cref="IAuthorizationService"/> to work.
    /// </summary>
    public class AuthorizationAutoConfigBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="ModerationAutoConfigBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordClient"/>.</exception>
        public AuthorizationAutoConfigBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.GuildAvailable += OnGuildAvailable;
            DiscordClient.LeftGuild += OnLeftGuild;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppingAsync()
        {
            DiscordClient.GuildAvailable -= OnGuildAvailable;
            DiscordClient.LeftGuild -= OnLeftGuild;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsStarted)
                OnStoppingAsync();

            base.Dispose(disposeManaged);
        }

        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// A <see cref="DiscordSocketClient"/> for interacting with, and receiving events from, the Discord API.
        /// </summary>
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnGuildAvailable(IGuild guild)
            => ExecuteOnScopedServiceAsync<IAuthorizationService>(x => x.AutoConfigureGuildAsync(guild));

        private Task OnLeftGuild(IGuild guild)
            => ExecuteOnScopedServiceAsync<IAuthorizationService>(x => x.UnConfigureGuildAsync(guild));
    }
}
