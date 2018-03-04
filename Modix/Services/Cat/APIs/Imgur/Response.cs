using Newtonsoft.Json;

namespace Modix.Services.Cat.APIs.Imgur
{
    public class Response
    {
        [JsonProperty("data")]
        public Album[] Album { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class Album
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }
    }

    public class Image
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}