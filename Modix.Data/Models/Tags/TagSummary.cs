using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a summary view of a tag for use in higher levels of the application.
    /// </summary>
    public class TagSummary
    {
        /// <summary>
        /// See <see cref="TagEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.CreateAction"/>.
        /// </summary>
        public TagActionBrief CreateAction { get; set; } = null!;

        /// <summary>
        /// See <see cref="TagEntity.DeleteAction"/>.
        /// </summary>
        public TagActionBrief? DeleteAction { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.Name"/>.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// See <see cref="TagEntity.Content"/>.
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// See <see cref="TagEntity.Uses"/>.
        /// </summary>
        public uint Uses { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.OwnerUser"/>.
        /// </summary>
        public GuildUserBrief? OwnerUser { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.OwnerRole"/>.
        /// </summary>
        public GuildRoleBrief? OwnerRole { get; set; }

        [ExpansionExpression]
        public static readonly Expression<Func<TagEntity, TagSummary>> FromEntityProjection
            = entity => new TagSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                CreateAction = entity.CreateAction.Project(TagActionBrief.FromEntityProjection),
                DeleteAction = (entity.DeleteAction == null)
                    ? null
                    : entity.DeleteAction.Project(TagActionBrief.FromEntityProjection),
                Name = entity.Name,
                Content = entity.Content,
                Uses = entity.Uses,
                OwnerUser = (entity.OwnerUser == null)
                    ? null
                    : entity.OwnerUser.Project(GuildUserBrief.FromEntityProjection),
                OwnerRole = (entity.OwnerRole == null)
                    ? null
                    : entity.OwnerRole.Project(GuildRoleBrief.FromEntityProjection),
            };
    }
}
