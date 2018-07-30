using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace Modix.Services.Utilities
{
    public static class GuildUserExtensions
    {
        public static string GetDisplayName(this IGuildUser user)
            => user.Nickname ?? user.Username;

        public static bool HasRole(this IGuildUser user, ulong roleId)
        {
            return user.RoleIds.Contains(roleId);
        }
    }
}
