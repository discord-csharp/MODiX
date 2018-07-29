using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationLogChannelMappingEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationLogChannelMappingBrief
    {
        /// <summary>
        /// See <see cref="ModerationLogChannelMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationLogChannelMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationLogChannelMappingEntity.LogChannelId"/>.
        /// </summary>
        public ulong LogChannelId { get; set; }

        internal static Expression<Func<ModerationLogChannelMappingEntity, ModerationLogChannelMappingBrief>> FromEntityProjection { get; }
            = entity => new ModerationLogChannelMappingBrief()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                LogChannelId = (ulong)entity.LogChannelId
            };
    }
}
