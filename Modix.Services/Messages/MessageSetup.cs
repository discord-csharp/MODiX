using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Messages
{
    /// <summary>
    /// Contains extension methods for configuring the messages functionality upon application startup.
    /// </summary>
    public static class MessageSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the messages functionality to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the message services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddMessages(this IServiceCollection services)
            => services
                .AddScoped<IMessageService, MessageService>();
    }
}
