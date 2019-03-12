using Discord;

namespace Modix.Services.Utilities
{
    public static class ModixEmbedBuilderExtensions
    {
        public static EmbedBuilder WithVerboseAuthor(this EmbedBuilder builder, IUser user)
        {
            return builder
                .WithAuthor($"{user.Username}#{user.Discriminator} ({user.Id})", user.GetDefiniteAvatarUrl());
        }
    }
}
