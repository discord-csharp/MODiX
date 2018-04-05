namespace Modix.Services.Animals.Cat.APIs.RandomCat
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Services.Animals;

    class RandomCatApi : IAnimalApi
    {
        public Task<Response> FetchAsync(MediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
