using Discord.Commands;
using Modix.Services.Fox;
using System;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Summary("Fox Related Commands")]
    public class FoxModule : ModuleBase
    {
        private readonly IFoxService _foxService;

        public FoxModule(IFoxService foxService)
        {
            _foxService = foxService;
        }

        [Command("fox", RunMode = RunMode.Async)]
        public async Task Fox()
        {
            string reply;
            try
            {
                reply = await _foxService.GetFoxPicture();
            }
            catch (TaskCanceledException)
            {
                reply = "Couldn't get a fox picture in time :(";
            }
            catch (Exception exc)
            {
                reply = $"Couldn't get a fox picture: {exc.Message}";
            }

            await ReplyAsync(reply);
        }
    }
}
