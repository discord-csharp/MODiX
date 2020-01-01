using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="GuildRoleEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class GuildRoleBrief
    {
        /// <summary>
        /// See <see cref="GuildRoleEntity.RoleId"/>.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// See <see cref="GuildRoleEntity.Name"/>.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// See <see cref="GuildRoleEntity.Position"/>.
        /// </summary>
        public int Position { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<GuildRoleEntity, GuildRoleBrief>> FromEntityProjection
            = entity => new GuildRoleBrief()
            {
                Id = entity.RoleId,
                Name = entity.Name,
                Position = entity.Position
            };
    }
}
