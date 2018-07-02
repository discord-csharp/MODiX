using System;
using System.Collections.Generic;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="Infraction"/> entities within an <see cref="IInfractionRepository"/>.
    /// </summary>
    public class InfractionSearchCriteria
    {
        /// <summary>
        /// A set of <see cref="Infraction.Type"/> values, defining the <see cref="Infraction"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<InfractionTypes> Types { get; set; }

        /// <summary>
        /// A <see cref="Infraction.SubjectId"/> value, defining the <see cref="Infraction"/> entities to be returned.
        /// </summary>
        public ulong? SubjectId { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="Infraction"/> entities to be returned,
        /// according to the <see cref="ModerationAction.Created"/> value of associated <see cref="ModerationAction"/> entities,
        /// with a <see cref="ModerationAction.Type"/> value of <see cref="ModerationActionTypes.InfractionCreated"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="Infraction"/> entities to be returned.
        /// according to the <see cref="ModerationAction.Created"/> value of associated <see cref="ModerationAction"/> entities,
        /// with a <see cref="ModerationAction.Type"/> value of <see cref="ModerationActionTypes.InfractionCreated"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A <see cref="Infraction.IsExpired"/> value, defining the <see cref="Infraction"/> entities to be returned.
        /// </summary>
        public bool? IsExpired { get; set; }

        /// <summary>
        /// A <see cref="Infraction.IsRescinded"/> value, defining the <see cref="Infraction"/> entities to be returned.
        /// </summary>
        public bool? IsRescinded { get; set; }
    }
}
