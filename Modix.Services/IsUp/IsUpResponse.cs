using Newtonsoft.Json;

namespace Modix.Services.IsUp
{
    public class IsUpResponse
    {
        [JsonProperty("deprecated")]
        public bool Deprecated { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("isitdown")]
        public bool Isitdown { get; set; }

        [JsonProperty("response_code")]
        public long? ResponseCode { get; set; }
    }
}
