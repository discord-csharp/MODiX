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
    public class CodePasteHandler
    {
        internal async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Content.Length < 750 &&
                arg.Content.Count(c => c == '\n') <= 30)
            {
                return;
            }

            //Make sure we don't have collisions with other modules
            //TODO: Make this less hacky
            if (arg.Content.StartsWith("!exec") || arg.Content.StartsWith("!eval") || arg.Content.StartsWith("!paste"))
            {
                return;
            }

            try
            {
                string url = await new CodePasteService().UploadCode(arg);

                await arg.Channel.SendMessageAsync($"Hey {arg.Author.Mention}, I took the liberty of uploading your overly-long message here:\n{url}");
                await arg.DeleteAsync();
            }
            catch (WebException ex)
            {
                await arg.Channel.SendMessageAsync($"I would have reuploaded your long message, but: {ex.Message}");
            }
        }
    }
}
