﻿using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    /// <summary>
    /// Listens for events from an <see cref="DiscordSocketClient"/> and dispatches them to the rest of the application,
    /// through an <see cref="IMessageDispatcher"/>.
    /// </summary>
    public class DiscordSocketListeningBehavior : IBehavior
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordSocketListeningBehavior"/> with the given dependencies.
        /// </summary>
        public DiscordSocketListeningBehavior(
            DiscordSocketClient discordSocketClient,
            IMessageDispatcher messageDispatcher)
        {
            DiscordSocketClient = discordSocketClient;
            MessageDispatcher = messageDispatcher;
        }

        /// <inheritdoc />
        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            DiscordSocketClient.ChannelCreated += OnChannelCreatedAsync;
            DiscordSocketClient.ChannelUpdated += OnChannelUpdatedAsync;
            DiscordSocketClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordSocketClient.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            DiscordSocketClient.JoinedGuild += OnJoinedGuildAsync;
            DiscordSocketClient.MessageDeleted += OnMessageDeletedAsync;
            DiscordSocketClient.MessageReceived += OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated += OnMessageUpdatedAsync;
            DiscordSocketClient.ReactionAdded += OnReactionAddedAsync;
            DiscordSocketClient.ReactionRemoved += OnReactionRemovedAsync;
            DiscordSocketClient.Ready += OnReadyAsync;
            DiscordSocketClient.RoleCreated += OnRoleCreatedAsync;
            DiscordSocketClient.RoleUpdated += OnRoleUpdatedAsync;
            DiscordSocketClient.UserBanned += OnUserBannedAsync;
            DiscordSocketClient.UserJoined += OnUserJoinedAsync;
            DiscordSocketClient.UserLeft += OnUserLeftAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            DiscordSocketClient.ChannelCreated -= OnChannelCreatedAsync;
            DiscordSocketClient.ChannelUpdated -= OnChannelUpdatedAsync;
            DiscordSocketClient.GuildAvailable -= OnGuildAvailableAsync;
            DiscordSocketClient.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
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
        internal protected DiscordSocketClient DiscordSocketClient { get; }

        /// <summary>
        /// A <see cref="IMessageDispatcher"/> used to dispatch discord notifications to the rest of the application.
        /// </summary>
        internal protected IMessageDispatcher MessageDispatcher { get; }

        private Task OnChannelCreatedAsync(SocketChannel channel)
        {
            MessageDispatcher.Dispatch(new ChannelCreatedNotification(channel));

            return Task.CompletedTask;
        }

        private Task OnChannelUpdatedAsync(SocketChannel oldChannel, SocketChannel newChannel)
        {
            MessageDispatcher.Dispatch(new ChannelUpdatedNotification(oldChannel, newChannel));

            return Task.CompletedTask;
        }

        private Task OnGuildAvailableAsync(SocketGuild guild)
        {
            MessageDispatcher.Dispatch(new GuildAvailableNotification(guild));

            return Task.CompletedTask;
        }

        private Task OnGuildMemberUpdatedAsync(SocketGuildUser oldMember, SocketGuildUser newMember)
        {
            MessageDispatcher.Dispatch(new GuildMemberUpdatedNotification(oldMember, newMember));

            return Task.CompletedTask;
        }

        private Task OnJoinedGuildAsync(SocketGuild guild)
        {
            MessageDispatcher.Dispatch(new JoinedGuildNotification(guild));

            return Task.CompletedTask;
        }

        private Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageDeletedNotification(message, channel));

            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsync(SocketMessage message)
        {
            MessageDispatcher.Dispatch(new MessageReceivedNotification(message));

            return Task.CompletedTask;
        }

        private Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageUpdatedNotification(oldMessage, newMessage, channel));

            return Task.CompletedTask;
        }

        private Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionAddedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionRemovedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            MessageDispatcher.Dispatch(new ReadyNotification());

            return Task.CompletedTask;
        }

        private Task OnRoleCreatedAsync(SocketRole role)
        {
            MessageDispatcher.Dispatch(new RoleCreatedNotification(role));

            return Task.CompletedTask;
        }

        private Task OnRoleUpdatedAsync(SocketRole oldRole, SocketRole newRole)
        {
            MessageDispatcher.Dispatch(new RoleUpdatedNotification(oldRole, newRole));

            return Task.CompletedTask;
        }

        private Task OnUserBannedAsync(SocketUser user, SocketGuild guild)
        {
            MessageDispatcher.Dispatch(new UserBannedNotification(user, guild));

            return Task.CompletedTask;
        }

        private Task OnUserJoinedAsync(SocketGuildUser guildUser)
        {
            MessageDispatcher.Dispatch(new UserJoinedNotification(guildUser));

            return Task.CompletedTask;
        }

        private Task OnUserLeftAsync(SocketGuildUser guildUser)
        {
            MessageDispatcher.Dispatch(new UserLeftNotification(guildUser));

            return Task.CompletedTask;
        }
    }
}
