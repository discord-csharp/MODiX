using Discord;
using Modix.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.AutoCodePaste
{
    public class CodePasteService
    {
        private const string Header = @"/*
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

        private const string _ApiReferenceUrl = "https://hastebin.com/";

        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        private async Task<string> UploadCode(string code)
        {
            var client = new HttpClient();
            var response = await client.PostAsync($"{_ApiReferenceUrl}documents", FormatUtilities.BuildContent(code));

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while posting code to Hastebin.");
            }

            string urlResponse = await response.Content.ReadAsStringAsync();
            string pasteKey = JObject.Parse(urlResponse)["key"].Value<string>();

            return $"{_ApiReferenceUrl}{pasteKey}.cs";
        }

        /// <summary>
        /// Uploads the code in the given message to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="msg">The Discord message to upload</param>
        /// <param name="code">The string to upload instead of message content</param>
        /// <returns>The URL to the newly created post</returns>
        internal async Task<string> UploadCode(IMessage msg, string code = null)
        {
            string formatted = String.Format(Header, $"{msg.Author.Username}#{msg.Author.DiscriminatorValue}",
                    msg.Channel.Name, DateTime.Now.ToString("dddd, MMMM d yyyy @ H:mm:ss"), msg.Id, FormatUtilities.FixIndentation(code ?? msg.Content));

            return await UploadCode(formatted);
        }
    }
}
