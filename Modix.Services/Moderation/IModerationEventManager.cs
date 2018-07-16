using System;
using System.Threading.Tasks;

using AsyncEvent;

namespace Modix.Services.Moderation
{
    // TODO: Gut this whole interface. It was originally supposed to support logging moderation actions to a mod-log channel,
    // thinking that this would need to be defined in the Modix project, but I realized that makes no sense, and it should just happen in ModerationService.
    /// <summary>
    /// Describes a service for dispatching moderation-related events at the application level.
    /// </summary>
    public interface IModerationEventManager
    {
        /// <summary>
        /// Occurs whenever a new moderation action is created
        /// </summary>
        event AsyncEventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        /// <summary>
        /// Raises the <see cref="ModerationActionCreated"/> event.
        /// </summary>
        /// <param name="argsFactory">A factory method that will asynchronously generate the args parameter for the event, if there are any subscribers.</param>
        /// <returns>A <see cref="Task"/> that will complete when the raised event has been handled.</returns>
        Task RaiseModerationActionCreatedAsync(Func<Task<ModerationActionCreatedEventArgs>> argsFactory);
    }
}
