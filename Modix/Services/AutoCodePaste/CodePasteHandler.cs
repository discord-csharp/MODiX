using Discord;
using Discord.WebSocket;
using Modix.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modix.Services.AutoCodePaste
{
    public class CodePasteHandler
    {
        const string Header = @"/*
    ______ _                       _   _____   _  _   
    |  _  (_)                     | | /  __ \_| || |_ 
    | | | |_ ___  ___ ___  _ __ __| | | /  \/_  __  _|
    | | | | / __|/ __/ _ \| '__/ _` | | |    _| || |_ 
    | |/ /| \__ \ (_| (_) | | | (_| | | \__/\_  __  _|
    |___/ |_|___/\___\___/|_|  \__,_|  \____/ |_||_|  
    
    Written By: {0} in #{1}
    Posted on {2}
    Message ID: {3}
*/

{4}";
        internal static async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Content.Length < 750) { return; }

            string content = String.Format(Header, $"{arg.Author.Username}#{arg.Author.DiscriminatorValue}",
                    arg.Channel.Name, DateTime.Now.ToString("dddd, MMMM d yyyy @ H:mm:ss"), arg.Id, FormatUtilities.FixIndentation(arg.Content));

            string url = await new CodePasteService().UploadCode(content);

            await arg.Channel.SendMessageAsync($"Sorry {arg.Author.Mention}, your message was a bit too long! Next time, paste long chunks of code elsewhere!\n{url}");
            await arg.DeleteAsync();
        }
    }
}
