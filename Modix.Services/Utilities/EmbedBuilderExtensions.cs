using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Modix.Services.Utilities
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithVerboseAuthor(this EmbedBuilder builder, IUser user)
        {
            return builder
                .WithAuthor($"{user.Username}#{user.Discriminator} ({user.Id})", user.GetAvatarUrl());
        }
        
    }
}
