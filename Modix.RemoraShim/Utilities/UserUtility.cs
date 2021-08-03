using Remora.Discord.API.Abstractions.Objects;

namespace Modix.RemoraShim.Utilities
{
    internal static class UserUtility
    {
        public static string GetAvatarUrl(IUser user)
        {
            if (user.Avatar is null)
                return $"https://cdn.discordapp.com/embed/avatars/{user.Discriminator % 5}.png";

            var extension = user.Avatar.HasGif
                ? "gif"
                : "png";

            return $"https://cdn.discordapp.com/avatars/{user.ID}/{user.Avatar.Value}.{extension}";
        }
    }
}
