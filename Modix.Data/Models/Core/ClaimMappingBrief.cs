using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="ClaimMappingEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class ClaimMappingBrief
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

        [ExpansionExpression]
        internal static readonly Expression<Func<ClaimMappingEntity, ClaimMappingBrief>> FromEntityProjection
            = entity => new ClaimMappingBrief()
            {
                Id = entity.Id,
                Type = entity.Type,
                RoleId = entity.RoleId,
                UserId = entity.UserId,
                Claim = entity.Claim,
            };
    }
}
