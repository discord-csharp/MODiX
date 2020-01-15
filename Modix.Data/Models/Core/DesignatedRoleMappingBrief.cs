using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a <see cref="DesignatedRoleMappingEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class DesignatedRoleMappingBrief
    {
        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.Role"/>.
        /// </summary>
        public GuildRoleBrief Role { get; set; } = null!;

        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.Type"/>
        /// </summary>
        public DesignatedRoleType Type { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<DesignatedRoleMappingEntity, DesignatedRoleMappingBrief>> FromEntityProjection
            = entity => new DesignatedRoleMappingBrief()
            {
                Id = entity.Id,
                Role = entity.Role.Project(GuildRoleBrief.FromEntityProjection),
                Type = entity.Type,
            };
    }

}
