using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes an object that receives logical events from repositories, regarding infractions.
    /// </summary>
    public interface IInfractionEventHandler
    {
        /// <summary>
        /// Signals to the handler that a new infraction was created.
        /// </summary>
        /// <param name="moderationActionId">The <see cref="InfractionEntity.Id"/> value of the infraction that was created.</param>
        /// <param name="data">A set of data that was used to create the infraction.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnInfractionCreatedAsync(long infractionId, InfractionCreationData data);
    }
}
