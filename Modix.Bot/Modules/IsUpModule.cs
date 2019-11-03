using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.IsUp;
using Modix.Services.Utilities;

namespace Modix.Modules
{

    [Name("Isup")]
    [Summary("Detects if a website has an outage")]
    [HelpTags("isup")]
    public class IsUpModule : ModuleBase
    {
        private readonly IsUpService _isUpService;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public IsUpModule(IsUpService isUpService,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            _isUpService = isUpService;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [Command("isup")]
        public async Task IsUp([Summary("Url to ping")] string url)
        {
            var message = await ReplyAsync("", embed: new EmbedBuilder()
                .WithTitle($"Checking status of {url}")
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.Orange)
                .Build());

            var resp = await _isUpService.GetIsUpResponseAsync(url);


            if (resp.StatusString != "OK")
            {
                await message.DeleteAsync();
                await ReplyAsync($"Error {resp.StatusString}: Something Went Wrong Querying the IsItDown API ");
                return;
            }

            EmbedBuilder builder;
            if (resp.Host == null || resp.ResponseCode == null)
            {
                 builder = new EmbedBuilder()
                    .WithTitle($"Host: { (resp.Host != null ? $"```{resp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(Context.User)
                    .WithColor(Color.Red)
                    .WithDescription($"Something went wrong querying: `{url}` Is that a valid URL?");
            }
            else
            {
                builder = new EmbedBuilder()
                    .WithTitle($"Host: { (resp.Host != null ? $"```{resp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(Context.User)
                    .WithColor(Color.Green)
                    .AddField("Status ", $"{(resp.Isitdown ? "❌" : "✅")} {resp.ResponseCode}", true);
            }

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.User, builder, async (e) =>
            {
                await message.ModifyAsync(a =>
                {
                    a.Content = string.Empty;
                    a.Embed = e.Build();
                });
                return message;
            });

            await Context.Message.DeleteAsync();
        }
    }
}
