using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

namespace Modix.Services.DuplicitMessage
{
    public static class DuplicitMessageSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the duplicit message removing to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the auto remove message services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddDuplicitMessageCheck(this IServiceCollection services)
            => services
               .AddSingleton<INotificationHandler<MessageReceivedNotification>, DuplicitMessageHandler>()
               .AddSingleton<INotificationHandler<MessageDeletedNotification>, DuplicitMessageHandler>()
               .AddSingleton<INotificationHandler<UserLeftNotification>, DuplicitMessageHandler>();
    }
}
