using System.Threading;
using System.Threading.Tasks;

namespace Modix.Common.Messaging
{
    /// <summary>
    /// Describes an object that receives and handles application-wide notifications from an <see cref="IMessagePublisher"/>.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification that this object handles.</typeparam>
    public interface INotificationHandler<TNotification>
        where TNotification : class
    {
        /// <summary>
        /// Handles a given notification.
        /// </summary>
        /// <param name="notification">The notification data to be handled.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task HandleNotificationAsync(
            TNotification notification,
            CancellationToken cancellationToken);
    }
}
