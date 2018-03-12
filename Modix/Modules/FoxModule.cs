using Discord.Commands;
using Modix.Services.Fox;
using System;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Summary("Fox Related Commands")]
    public class FoxModule : ModuleBase
    {
        [Command("fox", RunMode = RunMode.Async)]
        public async Task Fox()
        {
            string imageUrl;
            try
            {
                imageUrl = await new FoxService().GetFoxPicture();
            }
            catch (TaskCanceledException)
            {
                await ReplyAsync("Couldn't get a fox picture in time :(");
                return;
            }
            catch (Exception exc)
            {
                await ReplyAsync($"Couldn't get a fox picture: {exc.Message}");
                return;
            }

            await ReplyAsync(imageUrl);
        }
    }
}
