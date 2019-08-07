using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Modix.Bot.Extensions
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithDefaultFooter(this EmbedBuilder builder, IUser user)
        {
            builder.WithFooter(new EmbedFooterBuilder() { IconUrl = user.GetAvatarUrlOrDefault(), Text = $"Requested by {user.UsernameAndDiscrim()}" });
            return builder;
        }
    }
}
