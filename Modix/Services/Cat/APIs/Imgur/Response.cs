using Newtonsoft.Json;

namespace Modix.Services.Cat.APIs.Imgur
{
    public class Response
    {
        [JsonProperty("data")]
        public Image[] Images { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class Image
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("animated")]
        public bool Animated { get; set; }
    }
}