using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Messages.Discord;
using Modix.Services.NotificationDispatch;

namespace Modix.Services.Adapters
{
    public class DiscordAdapter : IBehavior
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly INotificationDispatchService _notificationDispatchService;

        public DiscordAdapter(
            DiscordSocketClient discordClient,
            INotificationDispatchService notificationDispatchService)
        {
            _discordClient = discordClient;
            _notificationDispatchService = notificationDispatchService;
        }

        public Task StartAsync()
        {
            _discordClient.MessageReceived += OnMessageReceived;
            _discordClient.MessageUpdated += OnMessageUpdated;
            _discordClient.MessageDeleted += OnMessageDeleted;
            _discordClient.ReactionAdded += OnReactionAdded;
            _discordClient.ReactionRemoved += OnReactionRemoved;
            _discordClient.UserJoined += OnUserJoined;
            _discordClient.UserBanned += OnUserBanned;
            return Task.CompletedTask;
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
            => _notificationDispatchService.PublishScopedAsync(new ReactionRemoved { Channel = channel, Message = message, Reaction = reaction });

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
            => _notificationDispatchService.PublishScopedAsync(new ReactionAdded { Channel = channel, Message = message, Reaction = reaction});

        private Task OnMessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage,
            ISocketMessageChannel channel)
            => _notificationDispatchService.PublishScopedAsync(new ChatMessageUpdated {  OldMessage = oldMessage, NewMessage = newMessage, Channel = channel });

        private Task OnMessageReceived(SocketMessage message)
            => _notificationDispatchService.PublishScopedAsync(new ChatMessageReceived { Message = message });

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
            => _notificationDispatchService.PublishScopedAsync(new ChatMessageDeleted { Message = message, Channel = channel });

        private Task OnUserJoined(SocketGuildUser user)
            => _notificationDispatchService.PublishScopedAsync(new UserJoined { Guild = user.Guild, User = user });

        private Task OnUserBanned(SocketUser bannedUser, SocketGuild bannedFromGuild)
            => _notificationDispatchService.PublishScopedAsync(new UserBanned { BannedUser = bannedUser, Guild = bannedFromGuild });

        public Task StopAsync()
        {
            _discordClient.MessageReceived -= OnMessageReceived;
            _discordClient.MessageUpdated -= OnMessageUpdated;
            _discordClient.MessageDeleted -= OnMessageDeleted;
            _discordClient.ReactionAdded -= OnReactionAdded;
            _discordClient.ReactionRemoved -= OnReactionRemoved;
            _discordClient.UserJoined -= OnUserJoined;
            _discordClient.UserBanned -= OnUserBanned;
            return Task.CompletedTask;
        }
    }
}
