﻿using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Mentions
{
    /// <summary>
    /// Implements a behavior that automatically performs an initial setup of mention mappings.
    /// </summary>
    public class MentionAutoConfigBehavior : BehaviorBase
    {
        public MentionAutoConfigBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordClient.RoleCreated += OnRoleCreatedAsync;
            DiscordClient.RoleUpdated += OnRoleUpdatedAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.GuildAvailable -= OnGuildAvailableAsync;

            return Task.CompletedTask;
        }

        /// <summary>
        /// A <see cref="DiscordSocketClient"/> for interacting with, and receiving events from, the Discord API.
        /// </summary>
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnGuildAvailableAsync(IGuild guild)
            => SelfExecuteRequest<IMentionService>(x => x.AutoConfigureGuildAsync(guild));

        private Task OnRoleCreatedAsync(IRole role)
            => SelfExecuteRequest<IMentionService>(x => x.AutoConfigureRoleAsync(role));

        private Task OnRoleUpdatedAsync(IRole role1, IRole role2)
            => SelfExecuteRequest<IMentionService>(x => x.AutoConfigureRoleAsync(role2));
    }
}