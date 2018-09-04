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
        /// A range of <see cref="ModerationActionEntity.Created"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="ModerationActionEntity.CreatedById"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public ulong? CreatedById { get; set; }
    }
}
