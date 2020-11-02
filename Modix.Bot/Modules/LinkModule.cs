using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Services.Utilities;

namespace Modix.Bot.Modules
{
    [Name("Link")]
    [Summary("Commands for working with links.")]
    public class LinkModule : ModuleBase
    {
        [Command("link")]
        [Alias("url", "uri", "shorten", "linkto")]
        [Summary("Shortens the provided link.")]
        public async Task LinkAsync(
            [Summary("The link to shorten.")]
                Uri uri)
        {
            var host = uri.Host;

            if (!_allowedHosts.Contains(host))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"Links to {host} cannot be shortened.")
                    .Build());

                return;
            }

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(Format.Url($"{host} (click here)", uri.ToString()))
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.LightGrey)
                .Build());

            await Context.Message.DeleteAsync();
        }

        private static readonly ImmutableArray<string> _allowedHosts
            = ImmutableArray.Create
            (
                "sharplab.io",
                "docs.microsoft.com",
                "www.docs.microsoft.com"
            );
    }
}
