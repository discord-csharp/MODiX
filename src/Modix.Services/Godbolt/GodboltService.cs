#nullable enable
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Modix.Services.Godbolt
{
    public class GodboltService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private const string GodboltApiScheme = "https://godbolt.org/api/compiler/dotnettrunk{0}coreclr/compile";

        public GodboltService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<string> CompileAsync(string code, string lang, string arguments, bool execute)
        {
            var request = new CompileRequest(lang, code);

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                request.Options.UserArguments = arguments;
            }

            if (execute)
            {
                request.Options.Filters.Execute = true;
            }

            var requestJson = JsonSerializer.Serialize(request, _jsonOptions);

            var client = HttpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "text/plain");

            var response = await client.PostAsync(string.Format(GodboltApiScheme, lang), new StringContent(requestJson, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Godbolt returns '{response.StatusCode}' for the request.");
            }

            return await response.Content.ReadAsStringAsync();
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
