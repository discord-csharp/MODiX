using Modix.Common.Messaging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Describes a notification that the application host is shutting down.
    /// </summary>
    public class HostStoppingNotification
        : INotification { }
}
