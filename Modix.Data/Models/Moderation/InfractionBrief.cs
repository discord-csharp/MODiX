using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    public class InfractionBrief
    {
        public long Id { get; set; }

        public InfractionType Type { get; set; }

        public TimeSpan? Duration { get; set; }

        public DiscordUserIdentity Subject { get; set; }

        internal static Expression<Func<InfractionEntity, InfractionBrief>> FromEntityProjection { get; }
            = entity => new InfractionBrief()
            {
                Id = entity.Id,
                Type = entity.Type,
                Duration = entity.Duration,
                Subject = new DiscordUserIdentity()
                {
                    UserId = entity.Subject.UserId,
                    Username = entity.Subject.Username,
                    Discriminator = entity.Subject.Discriminator,
                    Nickname = entity.Subject.Nickname
                },
            };
    }
}
