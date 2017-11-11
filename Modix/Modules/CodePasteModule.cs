using Discord;
using Discord.Commands;
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
            string response = await new CodePasteService().UploadCode(Context.Message, code);

            var builder = new EmbedBuilder()
                .WithTitle("Here's Your Paste")
                .WithDescription(response)
                .WithFooter(new EmbedFooterBuilder
                {
                    Text = $"Message Id: {Context.Message.Id}"
                })
                .WithColor(new Color(95, 186, 125));

            await ReplyAsync(Context.User.Mention, false, builder);
            await Context.Message.DeleteAsync();
        }
    }
}
