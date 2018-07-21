﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        internal static Expression<Func<ClaimMappingEntity, ClaimMappingBrief>> FromEntityProjection { get; }
            = entity => new ClaimMappingBrief()
            {
                Id = entity.Id,
                Type = entity.Type,
                GuildId = (ulong)entity.GuildId,
                RoleId = (ulong?)entity.RoleId,
                UserId = (ulong?)entity.UserId,
                Claim = entity.Claim
            };
    }
}
