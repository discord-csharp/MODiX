using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    public class ModerationActionSummary
    {
        public long Id { get; set; }

        public ModerationActionType Type { get; set; }

        public string Reason { get; set; }

        public DateTimeOffset Created { get; set; }

        public DiscordUserIdentity CreatedBy { get; set; }

        public InfractionBrief Infraction { get; set; }

        internal static Expression<Func<ModerationActionEntity, ModerationActionSummary>> FromEntityProjection { get; }
            = entity => new ModerationActionSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                Created = entity.Created,
                Reason = entity.Reason,
                CreatedBy = new DiscordUserIdentity()
                {
                    UserId = entity.CreatedBy.UserId,
                    Username = entity.CreatedBy.Username,
                    Discriminator = entity.CreatedBy.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Infraction = new InfractionBrief()
                {
                    Id = entity.Infraction.Id,
                    Type = entity.Infraction.Type,
                    Duration = entity.Infraction.Duration,
                    Subject = new DiscordUserIdentity()
                    {
                        UserId = entity.Infraction.Subject.UserId,
                        Username = entity.Infraction.Subject.Username,
                        Discriminator = entity.Infraction.Subject.Discriminator,
                        Nickname = entity.Infraction.Subject.Nickname
                    },
                }
            };
    }
}
