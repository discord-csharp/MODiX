using System.Collections.Generic;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationAction"/> entities within an <see cref="IModerationActionRepository"/>.
    /// </summary>
    public class ModerationActionSearchCriteria
    {
        /// <summary>
        /// A set of <see cref="ModerationAction.Type"/> values, defining the <see cref="ModerationAction"/> entities to be returned.
        /// </summary>
        public IEnumerable<ModerationActionType> Types { get; set; }

        /// <summary>
        /// A <see cref="ModerationAction.InfractionId"/> value, defining the <see cref="ModerationAction"/> entities to be returned.
        /// </summary>
        public long? InfractionId { get; set; }

        /// <summary>
        /// A range of <see cref="ModerationAction.Created"/> values, defining the <see cref="ModerationAction"/> entities to be returned.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="ModerationAction.CreatedById"/> value, defining the <see cref="ModerationAction"/> entities to be returned.
        /// </summary>
        public long? CreatedById { get; set; }

    }
}
