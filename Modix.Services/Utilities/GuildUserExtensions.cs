using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Modix.Services.Utilities
{
    public static class GuildUserExtensions
    {
        public static bool HasRole(this IGuildUser user, ulong roleId)
        {
            return user.HasRole(roleId);
        }
    }
}
