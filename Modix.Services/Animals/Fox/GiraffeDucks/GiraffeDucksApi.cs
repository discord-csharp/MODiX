using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Animals.Fox.GiraffeDucks
{
    public class GiraffeDucksApi : IAnimalApi
    {
        private const string FoxApiUrl = "https://giraffeduck.com/api/fox/direct";
        private readonly HttpClient _client;

        public GiraffeDucksApi()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            _client = new HttpClient(handler);
        }

        public async Task<Response> FetchAsync(MediaType mediaType, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync(FoxApiUrl, cancellationToken))
            {
                if (response.StatusCode == HttpStatusCode.Redirect)
                    return new UrlResponse(response.Headers.Location.AbsoluteUri);
            }

            return new UrlResponse();
        }
    }
}