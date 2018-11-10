using System;
using System.Linq.Expressions;

using Modix.Data.Projectables;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a. <see cref="ClaimMappingEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ClaimMappingSummary
    {
        /// <summary>
        /// See <see cref="ClaimMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.Type"/>.
        /// </summary>
        public ClaimMappingType Type { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.RoleId"/>.
        /// </summary>
        public ulong? RoleId { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.UserId"/>.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.Claim"/>.
        /// </summary>
        public AuthorizationClaim Claim { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.CreateAction"/>.
        /// </summary>
        public ConfigurationActionBrief CreateAction { get; set; }

        /// <summary>
        /// See <see cref="ClaimMappingEntity.DeleteAction"/>.
        /// </summary>
        public ConfigurationActionBrief DeleteAction { get; set; }

        internal static Expression<Func<ClaimMappingEntity, ClaimMappingSummary>> FromEntityProjection { get; }
            = entity => new ClaimMappingSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                GuildId = (ulong)entity.GuildId,
                RoleId = (ulong?)entity.RoleId,
                UserId = (ulong?)entity.UserId,
                Claim = entity.Claim,
                CreateAction = entity.CreateAction.Project(ConfigurationActionBrief.FromEntityProjection),
                DeleteAction = (entity.DeleteAction == null)
                    ? null
                    : entity.DeleteAction.Project(ConfigurationActionBrief.FromEntityProjection)
            };
    }
}
