using System;
using System.Collections.Generic;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="InfractionEntity"/> entities within an <see cref="IInfractionRepository"/>.
    /// </summary>
    public class InfractionSearchCriteria
    {
        /// <summary>
        /// A set of <see cref="InfractionEntity.Type"/> values, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<InfractionType> Types { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.SubjectId"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public long? SubjectId { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="InfractionEntity"/> entities to be returned,
        /// according to the <see cref="ModerationActionEntity.Created"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="InfractionEntity"/> entities to be returned.
        /// according to the <see cref="ModerationActionEntity.CreatedById"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.IsExpired"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public bool? IsExpired { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.IsRescinded"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public bool? IsRescinded { get; set; }
    }
}
