using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Animals.Cat.APIs.CaaS
{
    public class CaaSCatApi : IAnimalApi
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<Response> FetchAsync(MediaType mediaType, CancellationToken cancellationToken = default)
        {
            var url = "https://cataas.com/cat";

            if (mediaType == MediaType.Gif)
                url += "/gif";

            using (var response = await _httpClient.GetAsync(url, cancellationToken))
            {
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsByteArrayAsync();

                if (content != null)
                    return new ByteResponse(content);
            }

            return null;
        }
    }
}