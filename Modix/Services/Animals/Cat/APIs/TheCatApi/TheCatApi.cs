namespace Modix.Services.Animals.Cat.APIs.TheCatApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Modules;
    using Modix.Services.Animals;

    class TheCatApi : IAnimalApi
    {
        public Task<Response> FetchAsync(MediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
