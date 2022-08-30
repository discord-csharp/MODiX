﻿using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

using Serilog;

namespace Modix.Modules
{
    [ModuleHelp("Fun", "A bunch of miscellaneous, fun commands.")]
    [HelpTags("jumbo")]
    public class FunModule : InteractionModuleBase
    {
        private static readonly string[] _owoFaces = {"(・`ω´・)", ";;w;;", "owo", "UwU", ">w<", "^w^"};

        private const ushort MinimumAvatarSize = 16;
        private const ushort MaximumAvatarSize = 4096;

        public FunModule(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [SlashCommand("jumbo", "Jumbofy an emoji.")]
        public async Task JumboAsync(
            [Summary(description : "The emoji to jumbofy.")]
                string emoji)
        {
            var emojiUrl = EmojiUtilities.GetUrl(emoji);

            try
            {
                var client = HttpClientFactory.CreateClient();
                var req = await client.GetStreamAsync(emojiUrl);

                await FollowupWithFileAsync(req, Path.GetFileName(emojiUrl));
            }
            catch (HttpRequestException ex)
            {

                Log.Warning(ex, "Failed jumbofying emoji");
                await FollowupAsync($"Sorry {Context.User.Mention}, I don't recognize that emoji.");
            }
        }

        [SlashCommand("avatar", "Gets an avatar for a user.")]
        public async Task GetAvatarAsync(
            [Summary(description: "User that has the avatar.")]
                IUser user = null,
            [Summary(description: "Size for the avatar, defaults to 128.")]
            [MinValue(MinimumAvatarSize)]
            [MaxValue(MaximumAvatarSize)]
                ushort size = 128)
        {
            user ??= Context.User;

            try
            {
                size = Math.Clamp(size, MinimumAvatarSize, MaximumAvatarSize);

                var avatarUrl = user.GetDefiniteAvatarUrl(size);

                var embed = new EmbedBuilder()
                    .WithTitle($"{user.GetFullUsername()}'s avatar")
                    .WithImageUrl(avatarUrl)
                    .WithCurrentTimestamp()
                    .Build();

                await FollowupAsync(embed: embed);
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "Failed getting avatar for user {userId}", user.Id);
                await FollowupAsync($"Sorry {Context.User.Mention}, I couldn't get the avatar!");
            }
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("guild-avatar", "Gets a guild-specific avatar for a user.")]
        public async Task GetGuildAvatarAsync(
            [Summary(description: "User that has the avatar.")]
                IGuildUser user = null,
            [Summary(description: "Size for the avatar, defaults to 128.")]
                ushort size = 4096)
        {
            user ??= (IGuildUser)Context.User;

            try
            {
                size = Math.Clamp(size, MinimumAvatarSize, MaximumAvatarSize);

                var avatarUrl = user.GetGuildAvatarUrl(size: size) ?? user.GetDefiniteAvatarUrl(size);

                var embed = new EmbedBuilder()
                    .WithTitle($"{user.GetFullUsername()}'s avatar")
                    .WithImageUrl(avatarUrl)
                    .Build();

                await FollowupAsync(embed: embed);
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "Failed getting avatar for user {userId}", user.Id);
                await FollowupAsync($"Sorry {Context.User.Mention}, I couldn't get the avatar!");
            }
        }

        [SlashCommand("owo", "Owoifies the given message.")]
        public async Task OwoifyAsync([Summary(description: "The message to owoify.")] string message)
        {
            var owoMessage = message;

            owoMessage = Regex.Replace(owoMessage, "(?:r|l)", "w");
            owoMessage = Regex.Replace(owoMessage, "(?:R|L)", "W");
            owoMessage = Regex.Replace(owoMessage, "n([aeiou])", "ny$1");
            owoMessage = Regex.Replace(owoMessage, "N([aeiou])", "Ny$1");
            owoMessage = Regex.Replace(owoMessage, "N([AEIOU])", "Ny$1");
            owoMessage = Regex.Replace(owoMessage, "ove", "uv");
            owoMessage = Regex.Replace(owoMessage, "(?<!\\@)\\!+", " " + _owoFaces[new Random().Next(_owoFaces.Length)] + " ");

            await FollowupAsync(owoMessage, allowedMentions: AllowedMentions.None);
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
