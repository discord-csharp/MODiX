#nullable enable
using System.Net;
using Newtonsoft.Json;

namespace Modix.Services.IsUp
{

    public interface IIsUpResponse
    {
        string? StatusString { get; set; }
    }

    public class IsUpResponse : IIsUpResponse
    {

        [JsonProperty("deprecated")]
        public bool Deprecated { get; set; }

        [JsonProperty("host")]
        public string? Host { get; set; }

        [JsonProperty("isitdown")]
        public bool Isitdown { get; set; }

        [JsonProperty("response_code")]
        public long? ResponseCode { get; set; }

        public string? StatusString { get; set; }
    }
}
