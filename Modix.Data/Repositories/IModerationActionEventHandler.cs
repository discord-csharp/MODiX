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
        /// <param name="guildId">The Discord snowflake ID of the guild to which this moderation action applies.</param>
        /// <param name="moderationActionId">The <see cref="ModerationActionEntity.Id"/> value of the action that was created.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnModerationActionCreatedAsync(ulong guildId, long moderationActionId);
    }
}
