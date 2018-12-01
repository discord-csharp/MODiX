using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

namespace Modix.Services.Messaging
{
    public static class MessagingSetup
    {
        public static IServiceCollection AddModixMessaging(this IServiceCollection services)
            => services
                .AddSingleton<IMessageDispatcher, MessageDispatcher>()
                .AddScoped<IMessagePublisher, MessagePublisher>()
                .AddSingleton<IBehavior, DiscordSocketListeningBehavior>();
    }
}
