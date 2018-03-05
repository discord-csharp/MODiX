namespace Modix.Services.Cat.APIs.RandomCat
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class RandomCatApi : ICatApi
    {
        public Task<CatResponse> Fetch(CatMediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
