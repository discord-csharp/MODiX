using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    public class DesignatedChannelMappingBrief
    {
        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.ChannelDesignation"/>
        /// </summary>
        public ChannelDesignation ChannelDesignation { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.LogChannelId"/>.
        /// </summary>
        public ulong ChannelId { get; set; }

        internal static Expression<Func<DesignatedChannelMappingEntity, DesignatedChannelMappingBrief>> FromEntityProjection { get; }
            = entity => new DesignatedChannelMappingBrief()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                ChannelId = (ulong)entity.ChannelId,
                ChannelDesignation = entity.ChannelDesignation
            };
    }

}
