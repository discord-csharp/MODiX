using System.Collections.Generic;

namespace Modix.Services.Wikipedia
{
    public class WikipediaResponse
    {
        public WikipediaQuery Query { get; set; }
    }

    public class WikipediaQuery
    {
        public Dictionary<string, WikipediaPage> Pages { get; set; }
    }

    public class WikipediaPage
    {
        public string Extract { get; set; }
        public string Title { get; set; }
    }
}
