using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes an object that receives logical events from repositories, regarding moderation actions.
    /// </summary>
    public interface IModerationActionEventHandler
    {
        /// <summary>
        /// Signals to the handler that a new moderation action was created.
        /// </summary>
        /// <param name="moderationActionId">The <see cref="ModerationActionEntity.Id"/> value of the action that was created.</param>
        /// <param name="data">A set of data that was used to create the action.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnModerationActionCreatedAsync(long moderationActionId, ModerationActionCreationData data);
    }
}
