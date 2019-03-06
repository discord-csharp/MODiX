using System.Linq;
using Discord;

namespace Modix.Services.Utilities
{
    public static class GuildUserExtensions
    {
        public static string GetDisplayName(this IGuildUser user)
            => user.Nickname ?? user.Username;

        public static string GetDisplayNameWithDiscriminator(this IGuildUser user)
            => $"{user.Nickname ?? user.Username}#{user.Discriminator}";

        public static bool HasRole(this IGuildUser user, ulong roleId)
            => user.RoleIds.Contains(roleId);
    }
}
