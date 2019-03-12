using Discord;

namespace Modix.Services.Utilities
{
    public static class UserExtensions
    {
        public static string GetDisplayNameWithDiscriminator(this IUser user)
            => $"{user.Username}#{user.Discriminator}";

        public static string GetDefiniteAvatarUrl(this IUser user)
            => user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
    }
}
