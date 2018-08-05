using Discord;
using Modix.Data.Utilities;
using Modix.Services.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modix.Services.AutoCodePaste
{
    public class CodePasteService
    {
        private const string Header = @"
/*
    Written By: {0} in #{1}
    Posted on {2}
    Message ID: {3}
*/

{4}";

        private const string _ApiReferenceUrl = "https://hastebin.com/";
        private const string _FallbackApiReferenceUrl = "https://haste.charlesmilette.net/";
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCode(string code, string language = null)
        {
            var usingFallback = false;
            var content = FormatUtilities.BuildContent(code);
            HttpResponseMessage response;

            try
            {
                response = await client.PostAsync($"{_ApiReferenceUrl}documents", content);
            }
            catch (TaskCanceledException)
            {
                usingFallback = true;
                response = await client.PostAsync($"{_FallbackApiReferenceUrl}documents", content);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while posting code to Hastebin.");
            }

            var urlResponse = await response.Content.ReadAsStringAsync();
            var pasteKey = JObject.Parse(urlResponse)["key"].Value<string>();

            var domain = usingFallback ? _FallbackApiReferenceUrl : _ApiReferenceUrl;
            return $"{domain}{pasteKey}.{language ?? (FormatUtilities.GetCodeLanguage(code) ?? "cs")}";
        }

        /// <summary>
        /// Uploads the code in the given message to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="msg">The Discord message to upload</param>
        /// <param name="code">The string to upload instead of message content</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCode(IMessage msg, string code = null)
        {
            var formatted = string.Format(Header,
                $"{msg.Author.Username}#{msg.Author.DiscriminatorValue}", msg.Channel.Name,
                DateTime.Now.ToString("dddd, MMMM d yyyy @ H:mm:ss"), msg.Id, 
                FormatUtilities.FixIndentation(code ?? msg.Content));

            return await UploadCode(formatted);
        }

        public EmbedBuilder BuildEmbed(IUser user, string content, string url)
        {
            var cleanCode = FormatUtilities.FixIndentation(content);

            return new EmbedBuilder()
                .WithTitle("Your message was re-uploaded")
                .WithAuthor(user)
                .WithDescription(cleanCode.Trim().Truncate(200, 6))
                .AddField("Auto-Paste", url, true)
                .WithColor(new Color(95, 186, 125));
        }
    }
}
