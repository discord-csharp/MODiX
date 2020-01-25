using System.Linq;
using Modix.Data.Models.Core;

namespace Modix.Data.Extensions
{
    public static class GuildUserQueryExtensions
    {
        public static IQueryable<GuildUserEntity> WhereUserInGuild(this IQueryable<GuildUserEntity> source,
            ulong userId, ulong guildId)
        {
            return source.Where(x => x.UserId == userId && x.GuildId == guildId);
        }
    }
}
