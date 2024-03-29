using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="DesignatedChannelMappingEntity"/>.
    /// </summary>
    public class DesignatedChannelMappingCreationData
    {
        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.ChannelId"/>.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.Type"/>
        /// </summary>
        public DesignatedChannelType Type { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal DesignatedChannelMappingEntity ToEntity()
            => new DesignatedChannelMappingEntity()
            {
                GuildId = GuildId,
                ChannelId = ChannelId,
                Type = Type,
                CreateAction = new ConfigurationActionEntity()
                {
                    GuildId = GuildId,
                    Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = CreatedById
                }
            };
    }
}
