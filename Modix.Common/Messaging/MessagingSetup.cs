using Microsoft.Extensions.DependencyInjection;

namespace Modix.Common.Messaging
{
    public static class MessagingSetup
    {
        public static IServiceCollection AddModixMessaging(this IServiceCollection services)
            => services
                .AddSingleton<IMessageDispatcher, MessageDispatcher>()
                .AddScoped<IMessagePublisher, MessagePublisher>();
    }
}
