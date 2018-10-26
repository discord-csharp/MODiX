using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Preconditions;
using Serilog;

namespace Modix.Modules
{
    [Name("Fun"), Summary("A bunch of miscellaneous, fun commands")]
    public class FunModule : ModuleBase
    {
        [Command("jumbo"), Summary("Jumbofy an emoji")]
        public async Task Jumbo(string emoji)
        {
            string emojiUrl = null;

            if (Emote.TryParse(emoji, out Emote found))
            {
                emojiUrl = found.Url;
            }
            else
            {
                int codepoint = Char.ConvertToUtf32(emoji, 0);
                string codepointHex = codepoint.ToString("X").ToLower();

                emojiUrl = $"https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/{codepointHex}.png";
            }

            try
            {
                HttpClient client = new HttpClient();
                var req = await client.GetStreamAsync(emojiUrl);

                await Context.Channel.SendFileAsync(req, Path.GetFileName(emojiUrl), Context.User.Mention);
                
                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch (HttpRequestException)
                {
                    Log.Information("Couldn't delete message after jumbofying.");
                }
            }
            catch (HttpRequestException)
            {
                await ReplyAsync($"Sorry {Context.User.Mention}, I don't recognize that emoji.");
            }
        }

        [Command("wam"), Summary("Wams a user")]
        public async Task Wam([RequireInzanit]SocketGuildUser user)
        {
            await ReplyAsync($"WAM! {user.Mention} was WAMMED by {Context.User.Mention}");
        }
    }
}
