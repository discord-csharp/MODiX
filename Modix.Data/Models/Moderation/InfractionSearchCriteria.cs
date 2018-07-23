using System.Collections.Generic;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="InfractionEntity"/> entities within an <see cref="IInfractionRepository"/>.
    /// </summary>
    public class InfractionSearchCriteria
    {
        /// <summary>
        /// A <see cref="InfractionEntity.GuildId"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A set of <see cref="InfractionEntity.Type"/> values, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<InfractionType> Types { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.SubjectId"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public ulong? SubjectId { get; set; }

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
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="InfractionEntity"/> entities to be returned,
        /// according to the value of <see cref="InfractionEntity.Duration"/> and the <see cref="ModerationActionEntity.Created"/> value
        /// of <see cref="InfractionEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? ExpiresRange { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.RescindActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsRescinded { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}
