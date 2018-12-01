namespace Modix.Common.Messaging
{
    /// <summary>
    /// Identifies a class that describes an application-wide notification, which is dispatched by an <see cref="IMessagePublisher"/>
    /// and handled by <see cref="INotificationHandler{TNotification}"/> objects.
    /// </summary>
    public interface INotification { }
}
