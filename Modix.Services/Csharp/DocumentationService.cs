using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Modix.Services.Csharp
{
    public class DocumentationService
    {
        private const string ApiReferenceUrl = "https://docs.microsoft.com/api/apibrowser/dotnet/search?search=";

        public DocumentationService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<DocumentationApiResponse> GetDocumentationResultsAsync(string term)
        {
            var client = HttpClientFactory.CreateClient();
            var response = await client.GetAsync(ApiReferenceUrl + term);

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
