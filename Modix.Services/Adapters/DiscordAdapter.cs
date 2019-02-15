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
            _discordClient.MessageReceived += OnMessageReceived;
            _discordClient.MessageUpdated += OnMessageUpdated;
            _discordClient.ReactionAdded += OnReactionAdded;
            _discordClient.ReactionRemoved += OnReactionRemoved;
            _discordClient.UserJoined += OnUserJoined;
            return Task.CompletedTask;
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
            => PublishScoped(new ReactionRemoved { Channel = channel, Message = message, Reaction = reaction });

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
            => PublishScoped(new ReactionAdded { Channel = channel, Message = message, Reaction = reaction});

        private Task OnMessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage,
            ISocketMessageChannel channel)
            => PublishScoped(new ChatMessageUpdated {  OldMessage = oldMessage, NewMessage = newMessage, Channel = channel });

        private Task OnMessageReceived(SocketMessage message)
            => PublishScoped(new ChatMessageReceived { Message = message });

        private Task OnUserJoined(SocketGuildUser user)
            => PublishScoped(new UserJoined { Guild = user.Guild, User = user });

        private async Task PublishScoped(INotification message)
        {
            Log.Debug($"Beginning to publish a {message.GetType().Name} message");
            
            try
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
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception was thrown in the Discord MediatR adapter.");
            }

            Log.Debug($"Finished invoking {message.GetType().Name} handlers");
        }

        public Task StopAsync()
        {
            _discordClient.MessageReceived -= OnMessageReceived;
            _discordClient.MessageUpdated -= OnMessageUpdated;
            _discordClient.ReactionAdded -= OnReactionAdded;
            _discordClient.ReactionRemoved -= OnReactionRemoved;
            _discordClient.UserJoined -= OnUserJoined;
            return Task.CompletedTask;
        }
    }
}
