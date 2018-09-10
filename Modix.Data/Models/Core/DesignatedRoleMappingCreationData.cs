using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="DesignatedRoleMappingEntity"/>.
    /// </summary>
    public class DesignatedRoleMappingCreationData
    {
        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="DesignatedRoleMappingEntity.Type"/>
        /// </summary>
        public DesignatedRoleType Type { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal DesignatedRoleMappingEntity ToEntity()
            => new DesignatedRoleMappingEntity()
            {
                GuildId = (long)GuildId,
                RoleId = (long)RoleId,
                Type = Type,
                CreateAction = new ConfigurationActionEntity()
                {
                    GuildId = (long)GuildId,
                    Type = ConfigurationActionType.DesignatedRoleMappingCreated,
                    Created = DateTimeOffset.Now,
                    CreatedById = (long)CreatedById
                }
            };
    }
}
