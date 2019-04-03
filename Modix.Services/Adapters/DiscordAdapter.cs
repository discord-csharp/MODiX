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
            _discordClient.UserJoined += OnUserJoined;
            return Task.CompletedTask;
        }

        private Task OnUserJoined(SocketGuildUser user)
            => _notificationDispatchService.PublishScopedAsync(new UserJoined { Guild = user.Guild, User = user });

        public Task StopAsync()
        {
            _discordClient.UserJoined -= OnUserJoined;
            return Task.CompletedTask;
        }
    }
}
