namespace Modix.Services.Cat.APIs.TheCatApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class TheCatApi : ICatApi
    {
        public Task<CatResponse> Fetch(CatMediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
