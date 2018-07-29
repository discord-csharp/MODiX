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
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

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
                GuildId = (ulong)entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
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
                    // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                    //Type = entity.Infraction.Type,
                    Type = Enum.Parse<InfractionType>(entity.Infraction.Type.ToString()),
                    Reason = entity.Infraction.Reason,
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
