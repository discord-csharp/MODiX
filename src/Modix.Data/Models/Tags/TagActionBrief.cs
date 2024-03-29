using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a partial view of a tag action for use within the context of another projected model.
    /// </summary>
    public class TagActionBrief
    {
        /// <summary>
        /// See <see cref="TagActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="TagActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="TagActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief? CreatedBy { get; set; } = null!;

        [ExpansionExpression]
        internal static Expression<Func<TagActionEntity, TagActionBrief>> FromEntityProjection
            = entity => new TagActionBrief()
            {
                Id = entity.Id,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy == null
                    ? null
                    : entity.CreatedBy.Project(GuildUserBrief.FromEntityProjection),
            };
    }
}
