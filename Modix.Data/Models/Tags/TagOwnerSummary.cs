using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a summary view of the owner of a tag for use in higher levels of the application.
    /// </summary>
    public class TagOwnerSummary
    {
        /// <summary>
        /// See <see cref="TagOwnerEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="TagOwnerEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="TagOwnerEntity.User"/>.
        /// </summary>
        public GuildUserBrief User { get; set; }

        /// <summary>
        /// See <see cref="TagOwnerEntity.Role"/>.
        /// </summary>
        public GuildRoleBrief Role { get; set; }

        /// <summary>
        /// See <see cref="TagOwnerEntity.TagName"/>.
        /// </summary>
        public string TagName { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<TagOwnerEntity, TagOwnerSummary>> FromEntityProjection
            = entity => new TagOwnerSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                User = (entity.User == null)
                    ? null
                    : entity.User.Project(GuildUserBrief.FromEntityProjection),
                Role = (entity.Role == null)
                    ? null
                    : entity.Role.Project(GuildRoleBrief.FromEntityProjection),
                TagName = entity.TagName,
            };
    }
}
