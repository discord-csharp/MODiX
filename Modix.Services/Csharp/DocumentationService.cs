using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Modix.Services.Csharp
{
    public class DocumentationService
    {
        private const string ApiReferenceUrl = "https://docs.microsoft.com/api/apibrowser/dotnet/search?search=";

        public async Task<DocumentationApiResponse> GetDocumentationResultsAsync(string term)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(ApiReferenceUrl + term);

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the .NET Api docs.");
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DocumentationApiResponse>(jsonResponse);
        }
    }
}
