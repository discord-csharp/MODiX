using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a <see cref="ConfigurationActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ConfigurationActionSummary
    {
        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Type"/>.
        /// </summary>
        public ConfigurationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedBy"/>.
        /// </summary>
        public virtual GuildUserBrief CreatedBy { get; set; } = null!;

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.ClaimMapping"/>.
        /// </summary>
        public ClaimMappingBrief? ClaimMapping { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.DesignatedChannelMapping"/>.
        /// </summary>
        public DesignatedChannelMappingBrief? DesignatedChannelMapping { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.DesignatedRoleMapping"/>.
        /// </summary>
        public DesignatedRoleMappingBrief? DesignatedRoleMapping { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<ConfigurationActionEntity, ConfigurationActionSummary>> FromEntityProjection
            = entity => new ConfigurationActionSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
                CreatedBy = entity.CreatedBy.Project(GuildUserBrief.FromEntityProjection),
                ClaimMapping = (entity.ClaimMapping == null)
                    ? null
                    : entity.ClaimMapping.Project(ClaimMappingBrief.FromEntityProjection),
                DesignatedChannelMapping = (entity.DesignatedChannelMapping == null)
                    ? null
                    : entity.DesignatedChannelMapping.Project(DesignatedChannelMappingBrief.FromEntityProjection),
                DesignatedRoleMapping = (entity.DesignatedRoleMapping == null)
                    ? null
                    : entity.DesignatedRoleMapping.Project(DesignatedRoleMappingBrief.FromEntityProjection)
            };
    }
}
