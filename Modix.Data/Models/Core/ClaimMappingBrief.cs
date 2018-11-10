﻿using System;
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
                // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                //Type = entity.Type,
                Type = Enum.Parse<ClaimMappingType>(entity.Type.ToString()),
                RoleId = (ulong?)entity.RoleId,
                UserId = (ulong?)entity.UserId,
                // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                //Claim = entity.Claim,
                Claim = Enum.Parse<AuthorizationClaim>(entity.Claim.ToString())
            };
    }
}
