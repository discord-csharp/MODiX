using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationConfigEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationConfigSummary
    {
        /// <summary>
        /// See <see cref="ModerationConfigEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationConfigEntity.MuteRoleId"/>.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        internal static Expression<Func<ModerationConfigEntity, ModerationConfigSummary>> FromEntityProjection { get; }
            = entity => new ModerationConfigSummary()
            {
                GuildId = (ulong)entity.GuildId,
                MuteRoleId = (ulong)entity.MuteRoleId
            };
    }
}
