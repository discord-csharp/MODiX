using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository that creates <see cref="ModerationActionEntity"/> records in the datastore,
    /// and thus consumes <see cref="IModerationActionEventHandler"/> objects.
    /// </summary>
    public class ModerationActionEventRepositoryBase : RepositoryBase
    {
        /// <summary>
        /// Constructs a new <see cref="ModerationActionEventRepositoryBase"/> object, with the given injected dependencies.
        /// See <see cref="RepositoryBase(ModixContext)"/> for more details.
        /// </summary>
        public ModerationActionEventRepositoryBase(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers)
            : base(modixContext)
        {
            ModerationActionEventHandlers = moderationActionEventHandlers;
        }

        /// <summary>
        /// A set of <see cref="IModerationActionEventHandler"/> objects to receive information about moderation actions
        /// affected by this repository.
        /// </summary>
        internal protected IEnumerable<IModerationActionEventHandler> ModerationActionEventHandlers { get; }

        /// <summary>
        /// Notifies <see cref="ModerationActionEventHandlers"/> that a new <see cref="ModerationActionEntity"/> has been created.
        /// </summary>
        /// <param name="moderationAction">The <see cref="ModerationActionEntity"/> that was created.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected async Task RaiseModerationActionCreatedAsync(ModerationActionEntity moderationAction)
        {
            foreach (var handler in ModerationActionEventHandlers)
                await handler.OnModerationActionCreatedAsync(moderationAction.Id, new ModerationActionCreationData()
                {
                    GuildId = (ulong)moderationAction.GuildId,
                    Type = moderationAction.Type,
                    Created = moderationAction.Created,
                    CreatedById = (ulong)moderationAction.CreatedById
                });
        }
    }
}
