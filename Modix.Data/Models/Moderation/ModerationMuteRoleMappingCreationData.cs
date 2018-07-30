using System;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="ModerationMuteRoleMappingEntity"/>.
    /// </summary>
    public class ModerationMuteRoleMappingCreationData
    {
        /// <summary>
        /// See <see cref="ModerationMuteRoleMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationMuteRoleMappingEntity.MuteRoleId"/>.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal ModerationMuteRoleMappingEntity ToEntity()
            => new ModerationMuteRoleMappingEntity()
            {
                GuildId = (long)GuildId,
                MuteRoleId = (long)MuteRoleId,
                CreateAction = new ConfigurationActionEntity()
                {
                    Type = ConfigurationActionType.ModerationMuteRoleMappingCreated,
                    Created = DateTimeOffset.Now,
                    CreatedById = (long)CreatedById
                }
            };
    }
}
