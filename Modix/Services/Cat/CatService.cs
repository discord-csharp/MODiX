using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Modix.Services.Cat.APIs;
using Modix.Services.Cat.APIs.CaaS;
using Modix.Services.Cat.APIs.Imgur;

namespace Modix.Services.Cat
{
    using Modix.Services.Cat.APIs.RandomCat;
    using Modix.Services.Cat.APIs.TheCatApi;

    public interface ICatService
    {
        /// <summary>
        /// Gets a random Cat image URL
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CatResponse> Get(CatMediaType type, CancellationToken cancellationToken = default);
    }

    public class CatService : ICatService
    {
        private readonly List<ICatApi> _apis;

        public CatService()
        {
            _apis = new List<ICatApi>
            {
                new ImgurCatApi(),
                new CaaSCatApi(),
                new TheCatApi(),
                new RandomCatApi()
            };
        }

        public async Task<CatResponse> Get(CatMediaType type, CancellationToken cancellationToken)
        {
            // We may have many API providers, therefore iterate over all of them
            foreach (var api in _apis)
            {
                using (var token = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    // Fetch from the API
                    var response = await api.Fetch(type, token.Token);

                    // If the response is not null, we have a successful cat URL, return it
                    if (response.Success)
                        return response;
                }

                // Check if we've timed out before progressing to the next API
                cancellationToken.ThrowIfCancellationRequested();
            }

            // We got nothing, no cat at all
            return null;
        }
    }
}