using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Modix.Services.Wikipedia
{
    public class WikipediaResponse
    {
        [JsonPropertyName("query")]
        public WikipediaQuery Query { get; set; }
    }

    public class WikipediaQuery
    {
        [JsonPropertyName("pages")]
        public Dictionary<string, WikipediaPage> Pages { get; set; }
    }

    public class WikipediaPage
    {
        [JsonPropertyName("extract")]
        public string Extract { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
