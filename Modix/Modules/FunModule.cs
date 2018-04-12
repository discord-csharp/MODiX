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

namespace Modix.Modules
{
    [Name("Fun"), Summary("A bunch of miscellaneous, fun commands")]
    public class FunModule : ModuleBase
    {
        [Command("jumbo"), Summary("Jumbofy a server emoji")]
        public async Task Jumbo(string emoji)
        {
            Emote found;

            if (Emote.TryParse(emoji, out found))
            {
                HttpClient client = new HttpClient();
                await Context.Channel.SendFileAsync(await client.GetStreamAsync(found.Url), Path.GetFileName(found.Url), $"`Context.Message.Author.Username`");
                await Context.Message.DeleteAsync();
            }
            else
            {
                await ReplyAsync("I don't recognize that emoji.");
            }
        }
    }
}
