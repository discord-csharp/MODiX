using System.Collections.Generic;

using Modix.Data.Repositories;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationActionEntity"/> entities within an <see cref="IModerationActionRepository"/>.
    /// </summary>
    public class ModerationActionSearchCriteria
    {
        /// <summary>
        /// A <see cref="ModerationActionEntity.GuildId"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A set of <see cref="ModerationActionEntity.Type"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<ModerationActionType> Types { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="ModerationActionEntity"/> entities to be returned,
        /// according to the <see cref="ModerationActionEntity.Created"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.ModerationActionCreated"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// according to the <see cref="ModerationActionEntity.CreatedById"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.ModerationActionCreated"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }
    }
}
