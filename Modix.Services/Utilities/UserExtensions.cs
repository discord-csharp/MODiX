using System.Linq;
using Discord;
using Modix.Data.Models.Core;

namespace Modix.Services.Utilities
{
    public static class UserExtensions
    {
        public static string GetFullUsername(this IUser user)
            => $"{user.Username}#{user.Discriminator}";

        public static string GetFullUsername(this GuildUserSummary user)
            => $"{user.Username}#{user.Discriminator}";

        public static string GetFullUsername(this GuildUserBrief user)
            => $"{user.Username}#{user.Discriminator}";

        public static bool HasRole(this IGuildUser user, ulong roleId)
            => user.RoleIds.Contains(roleId);

        public static string GetDefiniteAvatarUrl(this IUser user, ushort size = 128)
            => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
    }
}
