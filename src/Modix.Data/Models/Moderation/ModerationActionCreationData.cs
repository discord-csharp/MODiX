using System;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="ModerationActionEntity"/>.
    /// </summary>
    public class ModerationActionCreationData
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }
    }
}
