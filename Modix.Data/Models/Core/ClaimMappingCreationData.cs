using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create an <see cref="ClaimMappingEntity"/>.
    /// </summary>
    public class ClaimMappingCreationData
    {
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
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal ClaimMappingEntity ToEntity()
            => new ClaimMappingEntity()
            {
                Type = Type,
                GuildId = GuildId,
                RoleId = RoleId,
                UserId = UserId,
                Claim = Claim,
                CreateAction = new ConfigurationActionEntity()
                {
                    GuildId = GuildId,
                    Type = ConfigurationActionType.ClaimMappingCreated,
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = CreatedById,
                }
            };
    }
}
