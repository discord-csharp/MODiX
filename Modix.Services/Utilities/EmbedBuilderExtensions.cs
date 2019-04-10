using Discord;

namespace Modix.Services.Utilities
{
    public static class ModixEmbedBuilderExtensions
    {
        public static EmbedBuilder WithVerboseAuthor(this EmbedBuilder builder, IUser user)
        {
            return builder
                .WithAuthor($"{user.GetFullUsername()} ({user.Id})", user.GetDefiniteAvatarUrl());
        }
    }
}
