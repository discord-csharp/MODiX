using System;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="ModerationLogChannelMappingEntity"/>.
    /// </summary>
    public class ModerationLogChannelMappingCreationData
    {
        /// <summary>
        /// See <see cref="ModerationLogChannelMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationLogChannelMappingEntity.LogChannelId"/>.
        /// </summary>
        public ulong LogChannelId { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal ModerationLogChannelMappingEntity ToEntity()
            => new ModerationLogChannelMappingEntity()
            {
                GuildId = (long)GuildId,
                LogChannelId = (long)LogChannelId,
                CreateAction = new ConfigurationActionEntity()
                {
                    GuildId = (long)GuildId,
                    Type = ConfigurationActionType.ModerationLogChannelMappingCreated,
                    Created = DateTimeOffset.Now,
                    CreatedById = (long)CreatedById
                }
            };
    }
}
