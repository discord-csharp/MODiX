#nullable enable
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modix.Services.Utilities;
using Newtonsoft.Json.Linq;

namespace Modix.Services;

public class PasteService(IHttpClientFactory httpClientFactory, ILogger<PasteService> logger)
{
    private const string PASTE_URL = "https://paste.mod.gg/";

    public async Task<string?> UploadPaste(string textToUpload)
    {
        var content = FormatUtilities.BuildContent(textToUpload);

        var client = httpClientFactory.CreateClient(HttpClientNames.TimeoutFiveSeconds);

        var response = await client.PostAsync($"{PASTE_URL}documents", content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();

            logger.LogError("Failed uploading paste to {Url}, failed with response {ResponseCode}, with body: {Body}",
                PASTE_URL,
                response.StatusCode,
                body);

            return null;
        }

        var urlResponse = await response.Content.ReadAsStringAsync();
        var pasteKey = JObject.Parse(urlResponse)["key"]?.Value<string>();

        return $"{PASTE_URL}{pasteKey}";
    }
}
