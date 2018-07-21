﻿using System;
using System.Linq.Expressions;

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
        /// See <see cref="ClaimMappingEntity.RescindAction"/>.
        /// </summary>
        public ConfigurationActionBrief RescindAction { get; set; }

        internal static Expression<Func<ClaimMappingEntity, ClaimMappingSummary>> FromEntityProjection { get; }
            = entity => new ClaimMappingSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                GuildId = (ulong)entity.GuildId,
                RoleId = (ulong?)entity.RoleId,
                UserId = (ulong?)entity.UserId,
                Claim = entity.Claim,
                CreateAction = new ConfigurationActionBrief()
                {
                    Id = entity.CreateAction.Id,
                    Type = entity.CreateAction.Type,
                    Created = entity.CreateAction.Created,
                    CreatedBy = new UserIdentity()
                    {
                        Id = (ulong)entity.CreateAction.CreatedBy.Id,
                        Username = entity.CreateAction.CreatedBy.Username,
                        Discriminator = entity.CreateAction.CreatedBy.Discriminator,
                        Nickname = entity.CreateAction.CreatedBy.Nickname
                    }
                },
                RescindAction = (entity.RescindAction == null) ? null
                    : new ConfigurationActionBrief()
                    {
                        Id = entity.RescindAction.Id,
                        Type = entity.RescindAction.Type,
                        Created = entity.RescindAction.Created,
                        CreatedBy = new UserIdentity()
                        {
                            Id = (ulong)entity.RescindAction.CreatedBy.Id,
                            Username = entity.RescindAction.CreatedBy.Username,
                            Discriminator = entity.RescindAction.CreatedBy.Discriminator,
                            Nickname = entity.RescindAction.CreatedBy.Nickname
                        }
                    }
            };
    }
}
