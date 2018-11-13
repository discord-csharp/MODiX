using System;
using System.Linq.Expressions;

using Modix.Data.Projectables;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a <see cref="DesignatedChannelMappingEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class DesignatedChannelMappingBrief
    {
        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.Channel"/>.
        /// </summary>
        public GuildChannelBrief Channel { get; set; }

        /// <summary>
        /// See <see cref="DesignatedChannelMappingEntity.Type"/>
        /// </summary>
        public DesignatedChannelType Type { get; set; }

        internal static Expression<Func<DesignatedChannelMappingEntity, DesignatedChannelMappingBrief>> FromEntityProjection { get; }
            = entity => new DesignatedChannelMappingBrief()
            {
                Id = entity.Id,
                Channel = entity.Channel.Project(GuildChannelBrief.FromEntityProjection),
                Type = entity.Type,
            };
    }

}
