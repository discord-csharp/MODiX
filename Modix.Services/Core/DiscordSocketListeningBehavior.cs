using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    /// <summary>
    /// Listens for events from an <see cref="IDiscordSocketClient"/> and dispatches them to the rest of the application,
    /// through an <see cref="IMessageDispatcher"/>.
    /// </summary>
    public class DiscordSocketListeningBehavior : IBehavior
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordSocketListeningBehavior"/> with the given dependencies.
        /// </summary>
        public DiscordSocketListeningBehavior(
            IDiscordSocketClient discordSocketClient,
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

        private Task OnGuildMemberUpdatedAsync(ISocketGuildUser oldMember, ISocketGuildUser newMember)
        {
            MessageDispatcher.Dispatch(new GuildMemberUpdatedNotification(oldMember, newMember));

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
            MessageDispatcher.Dispatch(new ReadyNotification());

            return Task.CompletedTask;
        }

        private Task OnRoleCreatedAsync(ISocketRole role)
        {
            MessageDispatcher.Dispatch(new RoleCreatedNotification(role));

            return Task.CompletedTask;
        }

        private Task OnRoleUpdatedAsync(ISocketRole oldRole, ISocketRole newRole)
        {
            MessageDispatcher.Dispatch(new RoleUpdatedNotification(oldRole, newRole));

            return Task.CompletedTask;
        }

        private Task OnUserBannedAsync(ISocketUser user, ISocketGuild guild)
        {
            MessageDispatcher.Dispatch(new UserBannedNotification(user, guild));

            return Task.CompletedTask;
        }

        private Task OnUserJoinedAsync(ISocketGuildUser guildUser)
        {
            MessageDispatcher.Dispatch(new UserJoinedNotification(guildUser));

            return Task.CompletedTask;
        }

        private Task OnUserLeftAsync(ISocketGuildUser guildUser)
        {
            MessageDispatcher.Dispatch(new UserLeftNotification(guildUser));

            return Task.CompletedTask;
        }
    }
}
