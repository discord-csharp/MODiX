using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Modix.Services.Csharp
{
    public class DocumentationService
    {
        private const string ApiReferenceUrl = "https://docs.microsoft.com/api/apibrowser/dotnet/search?api-version=0.2&search=";
        private const string ApiFilter = "&locale=en-us&$filter=monikers/any(t:%20t%20eq%20%27netcore-3.0%27)%20or%20monikers/any(t:%20t%20eq%20%27netframework-4.8%27)%20or%20monikers/any(t:%20t%20eq%20%27win-comm-toolkit-dotnet-stable%27)";

        public DocumentationService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<DocumentationApiResponse> GetDocumentationResultsAsync(string term)
        {
            var client = HttpClientFactory.CreateClient();
            var response = await client.GetAsync($"{ApiReferenceUrl}{term}{ApiFilter}");

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the .NET Api docs.");
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DocumentationApiResponse>(jsonResponse);
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
