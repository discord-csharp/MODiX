using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Modix.Services.Core
{
    public class StarboardService : BehaviorBase
    {
        public StarboardService(DiscordSocketClient discordClient, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        internal protected override Task OnStartingAsync()
        {
            DiscordClient.ReactionAdded += OnReactionAddedAsync;
            DiscordClient.ReactionRemoved += OnReactionRemovedAsync;
            DiscordClient.ReactionsCleared += OnReactionsClearedAsync;
            return Task.CompletedTask;
        }

        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.ReactionAdded -= OnReactionAddedAsync;
            DiscordClient.ReactionRemoved -= OnReactionRemovedAsync;
            DiscordClient.ReactionsCleared -= OnReactionsClearedAsync;
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

        /// <summary>
        /// Handles the reaction added event
        /// </summary>
        /// <param name="cachedEntity">Cached Entity Argument</param>
        /// <param name="channel">Discord Socket Channel ID</param>
        /// <param name="reaction">Discord Reaction</param>
        /// <returns></returns>
        private Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel,
            SocketReaction reaction)
            => SelfExecuteRequest(async x => await Task.CompletedTask);

        /// <summary>
        /// Handles the reaction removed event
        /// </summary>
        /// <param name="cachedEntity">Cached Entity Argument</param>
        /// <param name="channel">Discord Socket Channel ID</param>
        /// <param name="reaction">Discord Reaction</param>
        /// <returns></returns>
        private Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel,
            SocketReaction reaction)
            => SelfExecuteRequest(async x => await Task.CompletedTask);

        /// <summary>
        /// Handles the reaction cleared event
        /// </summary>
        /// <param name="cachedEntity">Cached Entity Argument</param>
        /// <param name="channel">Discord Socket Channel ID</param>
        /// <returns></returns>
        private Task OnReactionsClearedAsync(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel)
            => SelfExecuteRequest(async x => await Task.CompletedTask);

    }
}
