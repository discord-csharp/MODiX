using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a. <see cref="RoleClaimEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class RoleClaimSummary
    {
        /// <summary>
        /// See <see cref="RoleClaimEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.Claim"/>.
        /// </summary>
        public AuthorizationClaim Claim { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.CreateAction"/>.
        /// </summary>
        public ConfigurationActionBrief CreateAction { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.RescindAction"/>.
        /// </summary>
        public ConfigurationActionBrief RescindAction { get; set; }

        internal static Expression<Func<RoleClaimEntity, RoleClaimSummary>> FromEntityProjection { get; }
            = entity => new RoleClaimSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                RoleId = (ulong)entity.RoleId,
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
