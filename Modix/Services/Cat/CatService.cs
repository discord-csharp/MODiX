using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Modix.Services.Cat.APIs;
using Modix.Services.Cat.APIs.Imgur;

namespace Modix.Services.Cat
{
    public interface ICatService
    {
        /// <summary>
        /// Gets a random Cat image URL
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> Get(CatMediaType type, CancellationToken cancellationToken = default);
    }

    public class CatService : ICatService
    {
        private readonly List<ICatApi> _apis;

        public CatService(IHttpClient httpClient)
        {
            _apis = new List<ICatApi>
            {
                new ImgurCatApi(httpClient)
            };
        }

        public async Task<string> Get(CatMediaType type, CancellationToken cancellationToken)
        {
            // TODO Take into account type of media requested by user

            // We may have many API providers, therefore iterate over all of them
            foreach (var api in _apis)
            {
                // Fetch from the API
                var response = await api.Fetch(cancellationToken);

                // If the response is not null, we have a successful cat URL, return it
                if (!string.IsNullOrWhiteSpace(response))
                    return response;

                // Check if we've timed out before progressing to the next API
                cancellationToken.ThrowIfCancellationRequested();
            }

            // We got nothing, no cat at all
            return null;
        }
    }
}