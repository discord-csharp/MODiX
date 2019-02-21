using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.NotificationDispatch
{
    /// <summary>
    /// Contains extension methods for configuring the nodification dispatch feature upon application startup.
    /// </summary>
    public static class NotificationDispatchSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the notification dispatch service to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the notification dispatch services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddNotificationDispatch(this IServiceCollection services)
            => services
                .AddSingleton<INotificationDispatchService, NotificationDispatchService>();
    }
}
