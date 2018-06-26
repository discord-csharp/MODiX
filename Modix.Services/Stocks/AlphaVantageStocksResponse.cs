using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Modix.Services.Stocks
{
    public class AlphaVantageStocksResponse
    {
        [JsonProperty("Meta Data")]
        public Metadata Metadata { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> SeriesData { get; set; }
    }

    [JsonObject]
    public class Metadata
    {
        [JsonProperty("1. Information")]
        public string Information { get; set; }

        [JsonProperty("2. Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("3. Last Refreshed")]
        public DateTime LastRefreshed { get; set; }

        [JsonProperty("4. Interval")]
        public string Interval { get; set; }
    }
}
