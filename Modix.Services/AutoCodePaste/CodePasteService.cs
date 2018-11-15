using Discord;
using Modix.Data.Utilities;
using Modix.Services.Utilities;
using Newtonsoft.Json.Linq;
using System;
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

        private const string ApiReferenceUrl = "https://paste.mod.gg/";
        private const string FallbackApiReferenceUrl = "https://haste.charlesmilette.net/";
        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCodeAsync(string code, string language = null)
        {
            var usingFallback = false;
            var content = FormatUtilities.BuildContent(code);
            HttpResponseMessage response;

            try
            {
                response = await _client.PostAsync($"{ApiReferenceUrl}documents", content);
            }
            catch (TaskCanceledException)
            {
                usingFallback = true;
                response = await _client.PostAsync($"{FallbackApiReferenceUrl}documents", content);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content?.ReadAsStringAsync();
                throw new Exception($"{response.StatusCode} returned when calling {response.RequestMessage.RequestUri}. Response body: {body}");
            }

            var urlResponse = await response.Content.ReadAsStringAsync();
            var pasteKey = JObject.Parse(urlResponse)["key"].Value<string>();

            var domain = usingFallback ? FallbackApiReferenceUrl : ApiReferenceUrl;
            return $"{domain}{pasteKey}.{language ?? (FormatUtilities.GetCodeLanguage(code) ?? "cs")}";
        }

        /// <summary>
        /// Uploads the code in the given message to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="msg">The Discord message to upload</param>
        /// <param name="code">The string to upload instead of message content</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCodeAsync(IMessage msg, string code = null)
        {
            var formatted = string.Format(Header,
                $"{msg.Author.Username}#{msg.Author.DiscriminatorValue}", msg.Channel.Name,
                DateTimeOffset.UtcNow.ToString("dddd, MMMM d yyyy @ H:mm:ss"), msg.Id, 
                FormatUtilities.FixIndentation(code ?? msg.Content));

            return await UploadCodeAsync(formatted);
        }

        public EmbedBuilder BuildEmbed(IUser user, string content, string url)
        {
            var cleanCode = FormatUtilities.FixIndentation(content);

            return new EmbedBuilder()
                .WithTitle("Your message was re-uploaded")
                .WithAuthor(user)
                .WithDescription(cleanCode.Trim().Truncate(200, 6))
                .AddInlineField("Auto-Paste", url)
                .WithColor(new Color(95, 186, 125));
        }
    }
}
