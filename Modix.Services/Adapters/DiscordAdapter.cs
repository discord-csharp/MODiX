using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;
using Serilog;

namespace Modix.Services.Adapters
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
            => PublishScoped(new ChatMessageUpdated(oldMessage, newMessage, channel));

        private Task OnDiscordClientMessageReceived(SocketMessage message)
            => PublishScoped(new ChatMessageReceived(message));

        private async Task PublishScoped(INotification message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;

                // setup context for handlers
                var botUser = provider.GetRequiredService<ISelfUser>();
                await provider.GetRequiredService<IAuthorizationService>()
                    .OnAuthenticatedAsync(botUser);

                var mediator = provider.GetRequiredService<IMediator>();
                await mediator.Publish(message);
                Log.Debug("Done publishing message");
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
