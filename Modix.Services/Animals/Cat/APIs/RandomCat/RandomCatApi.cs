﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Animals.Cat.APIs.RandomCat
{
    class RandomCatApi : IAnimalApi
    {
        public Task<Response> FetchAsync(MediaType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}