using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Common.Messaging
{
    /// <summary>
    /// Defines configuration options for the behavior of the messaging system.
    /// </summary>
    public class MessagingOptions
    {
        /// <summary>
        /// The amount of time to wait for a <see cref="IMessageDispatcher.Dispatch{TNotification}(TNotification, TimeSpan?)"/> operation to complete,
        /// when no explicit timeout is specified.
        /// </summary>
        public TimeSpan? DispatchTimeout { get; set; }
    }

    [ServiceConfigurator]
    public class MessagingOptionsConfigurator
        : IServiceConfigurator
    {
        public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration)
            => services.AddOptions<MessagingOptions>()
                .Bind(configuration.GetSection("Common:Messaging"));
    }
}
