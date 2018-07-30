using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationMuteRoleMappingEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationMuteRoleMappingBrief
    {
        /// <summary>
        /// See <see cref="ModerationMuteRoleMappingEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationMuteRoleMappingEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationMuteRoleMappingEntity.MuteRoleId"/>.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        internal static Expression<Func<ModerationMuteRoleMappingEntity, ModerationMuteRoleMappingBrief>> FromEntityProjection { get; }
            = entity => new ModerationMuteRoleMappingBrief()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                MuteRoleId = (ulong)entity.MuteRoleId
            };
    }
}
