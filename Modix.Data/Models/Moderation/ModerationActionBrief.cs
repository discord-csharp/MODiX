using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a brief subset of the properties of a <see cref="ModerationActionEntity"/>.
    /// This is generally for use within other models, to refer to related users.
    /// </summary>
    public class ModerationActionBrief
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public DiscordUserIdentity CreatedBy { get; set; }

        internal static Expression<Func<ModerationActionEntity, ModerationActionBrief>> FromEntityProjection { get; }
            = entity => new ModerationActionBrief()
            {
                Id = entity.Id,
                Created = entity.Created,
                CreatedBy = new DiscordUserIdentity()
                {
                    UserId = entity.CreatedBy.UserId,
                    Username = entity.CreatedBy.Username,
                    Discriminator = entity.CreatedBy.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Reason = entity.Reason
            };
    }
}
