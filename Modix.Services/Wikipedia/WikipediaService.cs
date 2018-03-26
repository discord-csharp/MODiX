namespace Modix.Services.Wikipedia
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class WikipediaService
    {
        private const string WikipediaApiScheme = "https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&exintro&titles={0}&redirects=";

        public async Task<WikipediaResponse> GetWikipediaResultsAsync(string phrase)
        {
            var query = string.Join(" ", phrase);
            query = WebUtility.UrlEncode(query);
            var client = new HttpClient();
            var response = await client.GetAsync(string.Format(WikipediaApiScheme, query));

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the Wikipedia API.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WikipediaResponse>(jsonResponse);
        }
    }
}
