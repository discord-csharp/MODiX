using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="ModerationActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class ModerationActionSummary
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public UserIdentity CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Infraction"/>.
        /// </summary>
        public InfractionBrief Infraction { get; set; }

        internal static Expression<Func<ModerationActionEntity, ModerationActionSummary>> FromEntityProjection { get; }
            = entity => new ModerationActionSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                Created = entity.Created,
                Reason = entity.Reason,
                CreatedBy = new UserIdentity()
                {
                    Id = (ulong)entity.CreatedBy.Id,
                    Username = entity.CreatedBy.Username,
                    Discriminator = entity.CreatedBy.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Infraction = new InfractionBrief()
                {
                    Id = entity.Infraction.Id,
                    Type = entity.Infraction.Type,
                    Duration = entity.Infraction.Duration,
                    Subject = new UserIdentity()
                    {
                        Id = (ulong)entity.Infraction.Subject.Id,
                        Username = entity.Infraction.Subject.Username,
                        Discriminator = entity.Infraction.Subject.Discriminator,
                        Nickname = entity.Infraction.Subject.Nickname
                    },
                }
            };
    }
}
