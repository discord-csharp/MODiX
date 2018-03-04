using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Cat.APIs.CaaS
{
    public class CaaSCatApi : ICatApi
    {
        private readonly IHttpClient _httpClient;

        public CaaSCatApi(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CatResponse> Fetch(CatMediaType type, CancellationToken cancellationToken = default)
        {
            var url = "https://cataas.com/cat";

            if (type == CatMediaType.Gif)
                url += "/gif";

            using (var response = await _httpClient.GetAsync(url, cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();

                    if (content != null)
                        return new ByteCatResponse(content);
                }
            }

            return null;
        }
    }
}
