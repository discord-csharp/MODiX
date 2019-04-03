using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Messages.Modix;

namespace Modix.Services.AutoRemoveMessage
{
    /// <summary>
    /// Contains extension methods for configuring the auto remove message feature upon application startup.
    /// </summary>
    public static class AutoRemoveMessageSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the auto remove message service to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the auto remove message services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddAutoRemoveMessage(this IServiceCollection services)
            => services
                .AddScoped<IAutoRemoveMessageService, AutoRemoveMessageService>()
                .AddScoped<MediatR.INotificationHandler<RemovableMessageSent>, AutoRemoveMessageHandler>()
                .AddScoped<Common.Messaging.INotificationHandler<ReactionAddedNotification>, AutoRemoveMessageHandler>()
                .AddScoped<MediatR.INotificationHandler<RemovableMessageRemoved>, AutoRemoveMessageHandler>();
    }
}
