using Discord;
using Modix.Data.Models.Core;

namespace Modix.Services.Utilities
{
    public static class UserExtensions
    {
        public static string GetDisplayName(this IUser user)
        {
            if (user.GlobalName is not null)
                return user.GlobalName;

            if (user.DiscriminatorValue == 0)
                return user.Username;

            return $"{user.Username}#{user.Discriminator}";
        }

        public static string GetFullUsername(this GuildUserBrief user)
        {
            if (user.Discriminator is "0000" or "????")
                return user.Username;

            return $"{user.Username}#{user.Discriminator}";
        }

        public static string GetDefiniteAvatarUrl(this IUser user, ushort size = 128)
            => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
    }
}
