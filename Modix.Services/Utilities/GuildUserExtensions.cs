using System.Linq;
using Discord;

namespace Modix.Services.Utilities
{
    public static class GuildUserExtensions
    {
        public static bool HasRole(this IGuildUser user, ulong roleId)
        {
            return user.RoleIds.Contains(roleId);
        }
    }
}