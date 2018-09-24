using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Modix.Services.Utilities
{
    public static class ModixEmbedBuilderExtensions
    {
        public static EmbedBuilder WithVerboseAuthor(this EmbedBuilder builder, IUser user)
        {
            return builder
                .WithAuthor($"{user.Username}#{user.Discriminator} ({user.Id})", user.GetAvatarUrl());
        }

        public static string MessageLink(ulong guildId, ulong channelId, ulong messageId)
        {
            return $"https://discordapp.com/channels/{guildId}/{channelId}/{messageId}";
        }
        
    }
}
