using System;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create an <see cref="InfractionEntity"/>.
    /// </summary>
    public class InfractionCreationData
    {
        /// <summary>
        /// See <see cref="InfractionEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.SubjectId"/>.
        /// </summary>
        public ulong SubjectId { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.CreateActionId"/>.
        /// </summary>
        public long CreateActionId { get; set; }

        internal InfractionEntity ToEntity()
            => new InfractionEntity()
            {
                Type = Type,
                Duration = Duration,
                SubjectId = (long)SubjectId,
                CreateActionId = CreateActionId
            };
    }
}
