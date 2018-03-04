using Newtonsoft.Json;

namespace Modix.Services.Cat.APIs.Imgur
{
    public class Response
    {
        [JsonProperty("data")]
        public Image[] Images { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class Image
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}