using Discord;
using Discord.WebSocket;
using Modix.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modix.Services.AutoCodePaste
{
    public static class CodePasteHandler
    {
        internal static async Task MessageReceived(SocketMessage arg)
        {
            string code = arg.Content;
            int lines = code.Count(c => c == '\n');

            if (code.Contains("```"))
            {
                if (code.Length < 1000)
                {
                    return;
                }
            }
            else if (lines < 30 && code.Length < 1500)
            {
                return;
            }

            //Make sure we don't have collisions with other modules
            //TODO: Make this less hacky
            if (code.StartsWith("!exec") || code.StartsWith("!eval") || code.StartsWith("!paste"))
            {
                return;
            }

            try
            {
                string url = await new CodePasteService().UploadCode(arg);

                var embed = new EmbedBuilder()
                    .WithAuthor(arg.Author)
                    .WithDescription($"Your message was a bit long; next time, consider pasting it somewhere else or use the `!paste [code]` command.")
                    .AddInlineField("Auto-Paste", url)
                    .WithFooter(new EmbedFooterBuilder
                    {
                        Text = $"Message Id: {arg.Id}"
                    })
                    .WithColor(new Color(95, 186, 125));

                await arg.Channel.SendMessageAsync(arg.Author.Mention, false, embed);
                await arg.DeleteAsync();
            }
            catch (WebException ex)
            {
                await arg.Channel.SendMessageAsync($"I would have reuploaded your long message, but: {ex.Message}");
            }
        }
    }
}
