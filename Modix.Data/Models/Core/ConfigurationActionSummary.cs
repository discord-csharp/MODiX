﻿using System;
using System.Linq.Expressions;

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
        public virtual UserIdentity CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.ClaimMapping"/>.
        /// </summary>
        public ClaimMappingBrief ClaimMapping { get; set; }

        internal static Expression<Func<ConfigurationActionEntity, ConfigurationActionSummary>> FromEntityProjection { get; }
            = entity => new ConfigurationActionSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                Created = entity.Created,
                CreatedBy = new UserIdentity()
                {
                    Id = (ulong)entity.CreatedBy.Id,
                    Username = entity.CreatedBy.Username,
                    Discriminator = entity.CreatedBy.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                ClaimMapping = (entity.ClaimMapping == null) ? null
                    : new ClaimMappingBrief()
                    {
                        Id = entity.ClaimMapping.Id,
                        Type = entity.ClaimMapping.Type,
                        GuildId = (ulong)entity.ClaimMapping.GuildId,
                        RoleId = (ulong?)entity.ClaimMapping.RoleId,
                        UserId = (ulong?)entity.ClaimMapping.UserId,
                        Claim = entity.ClaimMapping.Claim
                    }
            };
    }
}
