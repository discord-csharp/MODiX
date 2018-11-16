using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;

namespace Modix.Adapters.Discord
{
    public class DiscordAdapter : IBehavior
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceProvider _serviceProvider;

        public DiscordAdapter(
            DiscordSocketClient discordClient,
            IServiceProvider serviceProvider)
        {
            _discordClient = discordClient;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync()
        {
            _discordClient.MessageReceived += OnDiscordClientMessageReceived;
            _discordClient.MessageUpdated += OnDiscordClientMessageUpdated;
            return Task.CompletedTask;
        }

        private Task OnDiscordClientMessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage,
            ISocketMessageChannel channel)
            => PublishScoped(new ChatMessageUpdated(oldMessage, newMessage, channel), newMessage.Author as IGuildUser);

        private Task OnDiscordClientMessageReceived(SocketMessage message)
            => PublishScoped(new ChatMessageReceived(message), message.Author as IGuildUser);

        private async Task PublishScoped(INotification message, IGuildUser user)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;

                // setup context for handlers
                await provider.GetRequiredService<IAuthorizationService>()
                    .OnAuthenticatedAsync(user);

                var mediator = provider.GetRequiredService<IMediator>();
                await mediator.Publish(message);
            }
        }

        public Task StopAsync()
        {
            _discordClient.MessageReceived -= OnDiscordClientMessageReceived;
            _discordClient.MessageUpdated -= OnDiscordClientMessageUpdated;
            return Task.CompletedTask;
        }
    }
}
