﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using StatsdClient;

namespace Modix.Services.Core
{
    /// <summary>
    /// Listens for events from an <see cref="IDiscordSocketClient"/> and dispatches them to the rest of the application,
    /// through an <see cref="IMessagePublisher"/>.
    /// </summary>
    public class DiscordSocketListeningBehavior : IBehavior
    {
        private readonly IDogStatsd _stats;

        /// <summary>
        /// Constructs a new <see cref="DiscordSocketListeningBehavior"/> with the given dependencies.
        /// </summary>
        public DiscordSocketListeningBehavior(
            IDiscordSocketClient discordSocketClient,
            IMessageDispatcher messageDispatcher,
            IDogStatsd stats = null)
        {
            DiscordSocketClient = discordSocketClient;
            MessageDispatcher = messageDispatcher;
            _stats = stats;
        }

        /// <inheritdoc />
        public Task StartAsync()
        {
            DiscordSocketClient.ChannelCreated += OnChannelCreatedAsync;
            DiscordSocketClient.ChannelUpdated += OnChannelUpdatedAsync;
            DiscordSocketClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordSocketClient.JoinedGuild += OnJoinedGuildAsync;
            DiscordSocketClient.MessageDeleted += OnMessageDeletedAsync;
            DiscordSocketClient.MessageReceived += OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated += OnMessageUpdatedAsync;
            DiscordSocketClient.ReactionAdded += OnReactionAddedAsync;
            DiscordSocketClient.ReactionRemoved += OnReactionRemovedAsync;
            DiscordSocketClient.Ready += OnReadyAsync;
            DiscordSocketClient.UserBanned += OnUserBannedAsync;
            DiscordSocketClient.UserJoined += OnUserJoinedAsync;
            DiscordSocketClient.UserLeft += OnUserLeftAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync()
        {
            DiscordSocketClient.ChannelCreated -= OnChannelCreatedAsync;
            DiscordSocketClient.ChannelUpdated -= OnChannelUpdatedAsync;
            DiscordSocketClient.GuildAvailable -= OnGuildAvailableAsync;
            DiscordSocketClient.JoinedGuild -= OnJoinedGuildAsync;
            DiscordSocketClient.MessageDeleted -= OnMessageDeletedAsync;
            DiscordSocketClient.MessageReceived -= OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated -= OnMessageUpdatedAsync;
            DiscordSocketClient.ReactionAdded -= OnReactionAddedAsync;
            DiscordSocketClient.ReactionRemoved -= OnReactionRemovedAsync;
            DiscordSocketClient.Ready -= OnReadyAsync;
            DiscordSocketClient.UserBanned -= OnUserBannedAsync;
            DiscordSocketClient.UserJoined -= OnUserJoinedAsync;
            DiscordSocketClient.UserLeft -= OnUserLeftAsync;

            return Task.CompletedTask;
        }

        /// <summary>
        /// The <see cref="DiscordSocketClient"/> to be listened to.
        /// </summary>
        internal protected IDiscordSocketClient DiscordSocketClient { get; }

        /// <summary>
        /// A <see cref="IMessageDispatcher"/> used to dispatch discord notifications to the rest of the application.
        /// </summary>
        internal protected IMessageDispatcher MessageDispatcher { get; }

        private Task OnChannelCreatedAsync(ISocketChannel channel)
        {
            MessageDispatcher.Dispatch(new ChannelCreatedNotification(channel));

            return Task.CompletedTask;
        }

        private Task OnChannelUpdatedAsync(ISocketChannel oldChannel, ISocketChannel newChannel)
        {
            MessageDispatcher.Dispatch(new ChannelUpdatedNotification(oldChannel, newChannel));

            return Task.CompletedTask;
        }

        private Task OnGuildAvailableAsync(ISocketGuild guild)
        {
            MessageDispatcher.Dispatch(new GuildAvailableNotification(guild));

            return Task.CompletedTask;
        }

        private Task OnJoinedGuildAsync(ISocketGuild guild)
        {
            MessageDispatcher.Dispatch(new JoinedGuildNotification(guild));

            return Task.CompletedTask;
        }

        private Task OnMessageDeletedAsync(ICacheable<IMessage, ulong> message, IISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageDeletedNotification(message, channel));

            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsync(ISocketMessage message)
        {
            MessageDispatcher.Dispatch(new MessageReceivedNotification(message));

            return Task.CompletedTask;
        }

        private Task OnMessageUpdatedAsync(ICacheable<IMessage, ulong> oldMessage, ISocketMessage newMessage, IISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageUpdatedNotification(oldMessage, newMessage, channel));

            return Task.CompletedTask;
        }

        private Task OnReactionAddedAsync(ICacheable<IUserMessage, ulong> message, IISocketMessageChannel channel, ISocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionAddedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnReactionRemovedAsync(ICacheable<IUserMessage, ulong> message, IISocketMessageChannel channel, ISocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionRemovedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            MessageDispatcher.Dispatch(ReadyNotification.Default);

            return Task.CompletedTask;
        }

        private Task OnUserBannedAsync(ISocketUser user, ISocketGuild guild)
        {
            try
            {
                MessageDispatcher.Dispatch(new UserBannedNotification(user, guild));
            }
            finally
            {
                UpdateGuildPopulationCounter(guild, "user_banned");
            }

            return Task.CompletedTask;
        }

        private Task OnUserJoinedAsync(ISocketGuildUser guildUser)
        {
            try
            {
                MessageDispatcher.Dispatch(new UserJoinedNotification(guildUser));
            }
            finally
            {
                UpdateGuildPopulationCounter(guildUser?.Guild, "user_joined");
            }

            return Task.CompletedTask;
        }

        private Task OnUserLeftAsync(ISocketGuildUser guildUser)
        {
            try
            {
                MessageDispatcher.Dispatch(new UserLeftNotification(guildUser));
            }
            finally
            {
                UpdateGuildPopulationCounter(guildUser?.Guild, "user_left");
            }

            return Task.CompletedTask;
        }

        private void UpdateGuildPopulationCounter(IGuild guild, string counterName)
        {
            if (!string.IsNullOrEmpty(guild?.Name))
            {
                var tags = new[] { "guild:" + guild.Name };

                _stats.Increment(counterName, tags: tags);

                if (guild is ISocketGuild socketGuild)
                {
                    _stats.Gauge("user_count", socketGuild.MemberCount, tags: tags);
                }
            }
        }
    }
}
