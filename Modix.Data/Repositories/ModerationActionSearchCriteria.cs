using System.Collections.Generic;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationActionEntity"/> entities within an <see cref="IModerationActionRepository"/>.
    /// </summary>
    public class ModerationActionSearchCriteria
    {
        /// <summary>
        /// A set of <see cref="ModerationActionEntity.Type"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public IEnumerable<ModerationActionTypes> Types { get; set; }

        /// <summary>
        /// A <see cref="ModerationActionEntity.InfractionId"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public long? InfractionId { get; set; }

        /// <summary>
        /// A range of <see cref="ModerationActionEntity.Created"/> values, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="ModerationActionEntity.CreatedById"/> value, defining the <see cref="ModerationActionEntity"/> entities to be returned.
        /// </summary>
        public long? CreatedById { get; set; }

    }
}
