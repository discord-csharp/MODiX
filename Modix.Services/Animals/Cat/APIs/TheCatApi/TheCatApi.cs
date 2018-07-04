using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Animals.Cat.APIs.TheCatApi
{
    class TheCatApi : IAnimalApi
    {
        public Task<Response> FetchAsync(MediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}