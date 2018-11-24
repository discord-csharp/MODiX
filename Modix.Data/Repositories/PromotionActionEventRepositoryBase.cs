using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Modix.Data.Messages;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository that creates <see cref="PromotionActionEntity"/> records in the datastore,
    /// and thus consumes <see cref="IPromotionActionEventHandler"/> objects.
    /// </summary>
    public abstract class PromotionActionEventRepositoryBase : RepositoryBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructs a new <see cref="PromotionActionEventRepositoryBase"/> object, with the given injected dependencies.
        /// See <see cref="RepositoryBase(ModixContext)"/>.
        /// </summary>
        public PromotionActionEventRepositoryBase(ModixContext modixContext, IMediator mediator)
            : base(modixContext)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Notifies <see cref="PromotionActionEventHandlers"/> that a new <see cref="PromotionActionEntity"/> has been created.
        /// </summary>
        /// <param name="promotionAction">The <see cref="PromotionActionEntity"/> that was created.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected Task RaisePromotionActionCreatedAsync(PromotionActionEntity promotionAction)
            => _mediator.Publish(new PromotionActionCreated
            {
                PromotionActionId = promotionAction.Id,
                PromotionActionCreationData = new PromotionActionCreationData
                {
                    Created = promotionAction.Created,
                    CreatedById = promotionAction.CreatedById,
                    GuildId = promotionAction.GuildId,
                    Type = promotionAction.Type
                }
            });
    }
}
