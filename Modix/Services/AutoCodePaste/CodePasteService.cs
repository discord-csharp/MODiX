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
        private string _ApiReferenceUrl = "https://hastebin.com/";

        /// <summary>
        /// Uploads a given piece of code to the service, and returns the URL to the post.
        /// </summary>
        /// <param name="code">The code to post</param>
        /// <returns>The URL to the newly created post</returns>
        public async Task<string> UploadCode(string code)
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
    }
}
