using System;
using System.Linq;
using System.Linq.Expressions;
using Modix.Data.Models.Core;

namespace Modix.Data.Extensions
{
    public static class MessageQueryExtensions
    {
        public static IQueryable<MessageEntity> WhereIsUserInGuild(this IQueryable<MessageEntity> source, ulong guildId, ulong userId)
        {
            return source.Where(x => x.GuildId == guildId && x.AuthorId == userId);
        }

        public static Expression<Func<MessageEntity, bool>> CountsTowardsParticipation()
        {
            return x => x.Channel.DesignatedChannelMappings.Any(d => d.Type == DesignatedChannelType.CountsTowardsParticipation);
        }

        public static IQueryable<MessageEntity> WhereCountsTowardsParticipation(this IQueryable<MessageEntity> source)
        {
            return source.Where(CountsTowardsParticipation());
        }
    }
}
