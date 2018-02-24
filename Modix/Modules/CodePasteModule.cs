using Discord.Commands;
using Modix.Services.AutoCodePaste;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("Code Paste"), Summary("Paste some code to the internet.")]
    public class CodePasteModule : ModuleBase
    {
        private CodePasteService _service;

        public CodePasteModule(CodePasteService service) {
            _service = service;
    }
        [Command("paste"), Summary("Paste the rest of your message to the internet, and return the URL.")]
        public async Task Run([Remainder] string code)
        {
            string url = await _service.UploadCode(Context.Message, code);
            var embed = _service.BuildEmbed(Context.User, code, url);

            await ReplyAsync(Context.User.Mention, false, embed);
            await Context.Message.DeleteAsync();
        }
    }
}
