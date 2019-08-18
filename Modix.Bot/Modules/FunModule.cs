using System;
using System.IO;
using System.Net.Http;
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

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
