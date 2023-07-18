using System;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create an <see cref="InfractionEntity"/>.
    /// </summary>
    public class InfractionCreationData
    {
        /// <summary>
        /// See <see cref="InfractionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Reason"/>
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.SubjectId"/>.
        /// </summary>
        public ulong SubjectId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal InfractionEntity ToEntity()
            => new InfractionEntity()
            {
                GuildId = GuildId,
                Type = Type,
                Reason = Reason,
                Duration = Duration,
                SubjectId = SubjectId,
                CreateAction = new ModerationActionEntity()
                {
                    GuildId = GuildId,
                    Type = ModerationActionType.InfractionCreated,
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = CreatedById
                }
            };
    }
}
