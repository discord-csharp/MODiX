using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Modules
{
    [Name("Fun")]
    [Summary("A bunch of miscellaneous, fun commands.")]
    [HelpTags("jumbo")]
    public class FunModule : ModuleBase
    {
        private static readonly string[] _owoFaces = {"(・`ω´・)", ";;w;;", "owo", "UwU", ">w<", "^w^"};

        public FunModule(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [Command("jumbo"), Summary("Jumbofy an emoji.")]
        public async Task JumboAsync(
            [Summary("The emoji to jumbofy.")]
                string emoji)
        {
            var emojiUrl = EmojiUtilities.GetUrl(emoji);

            try
            {
                var client = HttpClientFactory.CreateClient();
                var req = await client.GetStreamAsync(emojiUrl);

                await Context.Channel.SendFileAsync(req, Path.GetFileName(emojiUrl), Context.User.Mention);

                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch (HttpRequestException ex)
                {
                    Log.Warning(ex, "Couldn't delete message after jumbofying.");
                }
            }
            catch (HttpRequestException ex)
            {

                Log.Warning(ex, "Failed jumbofying emoji");
                await ReplyAsync($"Sorry {Context.User.Mention}, I don't recognize that emoji.");
            }
        }

        [Command("avatar"), Alias("av", "ava", "pfp"), Summary("Gets an avatar for a user")]
        public Task GetAvatarAsync(
                [Summary("Size for the avatar, defaults to 128")]
                ushort size = 128)
        {
            return GetAvatarAsync(Context.User, size);
        }

        [Command("avatar"), Alias("av", "ava", "pfp"), Summary("Gets an avatar for a user")]
        public async Task GetAvatarAsync(
            [Summary("User that has the avatar")]
                IUser user,
                [Summary("Size for the avatar, defaults to 128")]
                ushort size = 128)
        {

            try
            {
                const ushort MinimumSize = 128;
                const ushort MaximumSize = 512;

                // Set some minimum and maximum boundaries
                // anything under 128 and Discord won't fetch
                // the avatar. The upper is an arbitrary limit
                // to prevent chat from being spammed with large
                // avatars
                if (size < MinimumSize)
                {
                    size = MinimumSize;
                }
                else if (size > MaximumSize)
                {
                    size = MaximumSize;
                }

                var avatarUrl = user.GetAvatarUrl(size: size);

                if (string.IsNullOrWhiteSpace(avatarUrl))
                {
                    avatarUrl = user.GetDefaultAvatarUrl();
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"{user.Username}'s avatar")
                    .WithImageUrl(avatarUrl)
                    .WithCurrentTimestamp()
                    .WithFooter($"Requested by {Context.User.Username}")
                    .Build();

                await ReplyAsync(embed: embed);

                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch (HttpRequestException ex)
                {
                    Log.Warning(ex, "Couldn't delete message after getting avatar.");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "Failed getting avatar for user {userId}", user.Id);
                await ReplyAsync($"Sorry {Context.User.Mention}, I couldn't get the avatar!");
            }
        }

        [Command("owoify")]
        [Alias("owo")]
        [Summary("Owoifies the given message.")]
        public async Task OwoifyAsync([Remainder][Summary("The message to owoify.")] string message)
        {
            var owoMessage = message;

            owoMessage = Regex.Replace(owoMessage, "(?:r|l)", "w");
            owoMessage = Regex.Replace(owoMessage, "(?:R|L)", "W");
            owoMessage = Regex.Replace(owoMessage, "n([aeiou])", "ny$1");
            owoMessage = Regex.Replace(owoMessage, "N([aeiou])", "Ny$1");
            owoMessage = Regex.Replace(owoMessage, "N([AEIOU])", "Ny$1");
            owoMessage = Regex.Replace(owoMessage, "ove", "uv");
            owoMessage = Regex.Replace(owoMessage, "(?<!\\@)\\!+", " " + _owoFaces[new Random().Next(_owoFaces.Length)] + " ");

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(owoMessage)
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.Blue)
                .WithTimestamp(DateTime.UtcNow)
                .Build());
            await Context.Message.DeleteAsync();
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
