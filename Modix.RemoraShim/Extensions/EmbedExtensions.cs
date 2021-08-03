using System;
using System.Collections.Generic;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Remora.Discord.API.Abstractions.Objects
{
    public static class EmbedExtensions
    {
        public static IEnumerable<IEmbedField> EnumerateLongTextAsFieldBuilders(
                this string text,
                string fieldName,
                bool inline = false)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Cannot be empty", nameof(text));
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Cannot be empty", nameof(fieldName));

            for (var i = 0; i < text.Length; i += 1024)
            {
                yield return new EmbedField((i == 0) ? fieldName : $"{fieldName} (continued)", text[i..Math.Min(text.Length, (i + 1024))], inline);
            }
        }

        public static string GetJumpUrlForEmbed(this IPartialMessage message)
        {
            var messageLinkTxt = $"<#{message.ChannelID.Value.Value}> (click here)";
            return $"[{messageLinkTxt}]({message.GetMessageLink()})";
        }

        public static string GetMessageLink(this IPartialMessage message)
        {
            //https://discord.com/channels/143867839282020352/569261465463160900/871037390226083883
            return $"https://discord.com/channels/{message.GuildID.Value.Value}/{message.ChannelID.Value.Value}/{message.ID.Value.Value}";
        }

        public static Optional<IEmbedAuthor> WithUserAsAuthor(this IUser? user, string? extra = null)
        {
            if (user == null)
            {
                return new Optional<IEmbedAuthor>();
            }

            var suffix = string.Empty;

            if (!string.IsNullOrWhiteSpace(extra))
            {
                suffix = $" ({extra})";
            }

            return new EmbedAuthor(user.GetFullUsername() + suffix, user.GetDefiniteAvatarUrl());
        }

        public static string GetDefiniteAvatarUrl(this IUser user)
        {
            if (user.Avatar == default)
            {
                return $"https://cdn.discordapp.com/embed/avatars/{user.Discriminator % 5}.png";
            }

            return $"https://cdn.discordapp.com/avatars/{user.ID.Value}/{user.Avatar.Value}.png?size=512";
        }

        public static string GetFullUsername(this IUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }
    }
}
