using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Common.Messaging
{
    /// <summary>
    /// Describes an object that dispatches application-wide notifications.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatches a notification to be handled by all registered <see cref="INotificationHandler{TNotification}"/> objects.
        /// on a new logical application thread. I.E. handlers for the notification are executed within a new <see cref="IServiceScope"/>
        /// and synchronized through a new <see cref="Task"/> that will run in parallel to the current one, on the <see cref="ThreadPool"/>.
        /// Note that exceptions thrown by and handler of the notification will be logged, and will not affect the execution of other handlers for the notification.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification to be dispatched.</typeparam>
        /// <param name="notification">The notification data to be dispatched.</param>
        void Dispatch<TNotification>(TNotification notification) where TNotification : INotification;
    }

    /// <inheritdoc />
    public class MessageDispatcher : IMessageDispatcher
    {
        /// <summary>
        /// Constructs a new <see cref="MessageDispatcher"/> with the given dependencies.
        /// </summary>
        public MessageDispatcher(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc />
        public void Dispatch<TNotification>(TNotification notification) where TNotification : INotification
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            _ = DispatchAsync(notification);
        }

        /// <summary>
        /// An <see cref="IServiceScopeFactory"/> used to generate service scopes for dispatched messages to be processed within.
        /// </summary>
        internal protected IServiceScopeFactory ServiceScopeFactory { get; }

        // For testing
        internal async Task DispatchAsync<TNotification>(TNotification notification) where TNotification : INotification
        {
            try
            {
                using (var serviceScope = ServiceScopeFactory.CreateScope())
                {
                    var handlerTasks = new List<Task>();
                    foreach (var handler in serviceScope.ServiceProvider.GetServices<INotificationHandler<TNotification>>())
                    {
                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                await handler.HandleNotificationAsync(notification);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "An unexpected error occurred within a handler for a dispatched message: {notification}", notification);
                            }
                        });
                        handlerTasks.Add(task);
                    }

                    await Task.WhenAll(handlerTasks);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while dispatching a notification: {notification}", notification);
            }
        }
    }
}
