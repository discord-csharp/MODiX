#nullable enable
using Newtonsoft.Json;

namespace Modix.Services.IsUp
{
    public class IsUpResponseModel
    {
        [JsonProperty("deprecated")]
        public bool Deprecated { get; set; }

        [JsonProperty("host")]
        public string? Host { get; set; }

        [JsonProperty("isitdown")]
        public bool IsSiteDown { get; set; }

        [JsonProperty("response_code")]
        public long? ResponseCode { get; set; }
    }
}
