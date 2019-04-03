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
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
