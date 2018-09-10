using System.Threading.Tasks;

using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes an object that receives logical events from repositories, regarding promotion actions.
    /// </summary>
    public interface IPromotionActionEventHandler
    {
        /// <summary>
        /// Signals to the handler that a new promotion action was created.
        /// </summary>
        /// <param name="promotionActionId">The <see cref="PromotionActionEntity.Id"/> value of the action that was created.</param>
        /// <param name="data">A set of data that was used to create the action.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnPromotionActionCreatedAsync(long promotionActionId, PromotionActionCreationData data);
    }
}
