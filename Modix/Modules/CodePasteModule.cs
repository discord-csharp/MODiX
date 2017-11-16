using Discord;
using Discord.Commands;
using Modix.Data.Utilities;
using Modix.Services.AutoCodePaste;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("Code Paste"), Summary("Paste some code to the internet.")]
    public class CodePasteModule : ModuleBase
    {
        [Command("paste"), Summary("Paste the rest of your message to the internet, and return the URL.")]
        public async Task Run([Remainder] string code)
        {
            string url = await CodePasteService.UploadCode(Context.Message, code);
            var embed = CodePasteService.BuildEmbed(Context.User, code, url);

            await ReplyAsync(Context.User.Mention, false, embed);
            await Context.Message.DeleteAsync();
        }
    }
}
