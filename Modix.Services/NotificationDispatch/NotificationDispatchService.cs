using System;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Core;
using Serilog;

namespace Modix.Services.NotificationDispatch
{
    /// <summary>
    /// Defines a service to manage notification dispatch.
    /// </summary>
    public interface INotificationDispatchService
    {
        /// <summary>
        /// Publishes the supplied notification within a scope.
        /// </summary>
        /// <param name="notification">The notification to be published.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task PublishScopedAsync(INotification notification);
    }

    /// <inheritdoc />
    internal class NotificationDispatchService : INotificationDispatchService
    {
        public NotificationDispatchService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public async Task PublishScopedAsync(INotification notification)
        {
            Log.Debug($"Beginning to publish a {notification.GetType().Name} message");

            try
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var provider = scope.ServiceProvider;

                    // setup context for handlers
                    var botUser = provider.GetRequiredService<ISelfUser>();
                    await provider.GetRequiredService<IAuthorizationService>()
                        .OnAuthenticatedAsync(botUser);

                    var mediator = provider.GetRequiredService<IMediator>();
                    await mediator.Publish(notification);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception was thrown in the Discord MediatR adapter.");
            }

            Log.Debug($"Finished invoking {notification.GetType().Name} handlers");
        }

        protected IServiceProvider ServiceProvider { get; }
    }
}
