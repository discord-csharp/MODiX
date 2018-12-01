using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Mentions
{
    /// <summary>
    /// Provides a summary view of a mention mapping configuration.
    /// </summary>
    public class MentionMappingSummary
    {
        /// <summary>
        /// See <see cref="MentionMappingEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.Role"/>.
        /// </summary>
        public GuildRoleBrief Role { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.Mentionability"/>.
        /// </summary>
        public MentionabilityType Mentionability { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.MinimumRank"/>.
        /// </summary>
        public GuildRoleBrief MinimumRank { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<MentionMappingEntity, MentionMappingSummary>> FromEntityProjection
            = entity => new MentionMappingSummary
            {
                RoleId = entity.RoleId,
                Role = (entity.Role == null)
                    ? null
                    : entity.Role.Project(GuildRoleBrief.FromEntityProjection),
                GuildId = entity.GuildId,
                Mentionability = entity.Mentionability,
                MinimumRank = (entity.MinimumRank == null)
                    ? null
                    : entity.MinimumRank.Project(GuildRoleBrief.FromEntityProjection),
            };
    }
}
