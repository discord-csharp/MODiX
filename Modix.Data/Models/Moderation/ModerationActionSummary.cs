using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;
using Modix.Data.Projectables;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationActionSummary
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserIdentity CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Infraction"/>.
        /// </summary>
        public InfractionBrief Infraction { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.DeletedMessage"/>.
        /// </summary>
        public DeletedMessageBrief DeletedMessage { get; set; }

        internal static Expression<Func<ModerationActionEntity, ModerationActionSummary>> FromEntityProjection { get; }
            = entity => new ModerationActionSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                //Type = entity.Type,
                Type = Enum.Parse<ModerationActionType>(entity.Type.ToString()),
                Created = entity.Created,
                CreatedBy = entity.CreatedBy.Project(GuildUserIdentity.FromEntityProjection),
                Infraction = (entity.Infraction == null)
                    ? null
                    : entity.Infraction.Project(InfractionBrief.FromEntityProjection),
                DeletedMessage = (entity.DeletedMessage == null)
                    ? null
                    : entity.DeletedMessage.Project(DeletedMessageBrief.FromEntityProjection)
            };
    }
}
