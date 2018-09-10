using System;
using System.Linq.Expressions;

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
        public GuildRoleBrief Role { get; set; }

        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.Type"/>
        /// </summary>
        public DesignatedRoleType Type { get; set; }

        internal static Expression<Func<DesignatedRoleMappingEntity, DesignatedRoleMappingBrief>> FromEntityProjection { get; }
            = entity => new DesignatedRoleMappingBrief()
            {
                Id = entity.Id,
                Role = new GuildRoleBrief()
                {
                    Id = (ulong)entity.Role.RoleId,
                    Name = entity.Role.Name,
                    Position = entity.Role.Position
                },
                Type = entity.Type
            };
    }

}
