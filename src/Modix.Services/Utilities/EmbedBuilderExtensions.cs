using Discord;

namespace Modix.Services.Utilities
{
    public static class ModixEmbedBuilderExtensions
    {
        public static EmbedBuilder WithUserAsAuthor(this EmbedBuilder builder, IUser user, string extra = null)
        {
            var suffix = string.Empty;

            if (!string.IsNullOrWhiteSpace(extra))
            {
                suffix = $" ({extra})";
            }

            return builder
                .WithAuthor(user.GetDisplayName() + suffix, user.GetDefiniteAvatarUrl());
        }
    }
}
