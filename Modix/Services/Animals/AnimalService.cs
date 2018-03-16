namespace Modix.Services.Animals
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Modules;
    using Modix.Services.Animals.Cat.APIs.CaaS;
    using Modix.Services.Animals.Cat.APIs.Imgur;
    using Modix.Services.Animals.Fox.GiraffeDucks;

    public interface IAnimalService
    {
        /// <summary>
        /// Gets an image
        /// </summary>
        /// <param name="animalType">The type of animal we want to see</param>
        /// <param name="mediaType">The type of media we want: an animated gif or a still picture</param>
        /// <param name="cancellationToken">The cancellation token for our time limit</param>
        /// <returns></returns>
        Task<Response> Get(AnimalType animalType, MediaType mediaType, CancellationToken cancellationToken = default);
    }

    public class AnimalService : IAnimalService
    {
        private readonly List<IAnimalApi> _catApis;
        private readonly List<IAnimalApi> _foxApis;
        //private readonly List<IAnimalApi> _dogApis;

        public AnimalService()
        {
            // Add APIs for animals into these lists
            _catApis = new List<IAnimalApi>
            {
                new ImgurCatApi(),
                new CaaSCatApi(),
            };

            _foxApis = new List<IAnimalApi>
            {
                new GiraffeDucksApi()
            };

            /*_dogApis = new List<IAnimalApi>
            {

            };*/
        }

        public async Task<Response> Get(AnimalType animalType, MediaType mediaType, CancellationToken cancellationToken = default)
        {
            var apisUsed = new List<IAnimalApi>();

            switch (animalType)
            {
                case AnimalType.Cat:
                    apisUsed = _catApis;
                    break;
                case AnimalType.Fox:
                    apisUsed = _foxApis;
                    break;
                //case AnimalType.Dog:
                //    apisUsed = _dogApis;
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(animalType), animalType, null);
            }

            // We may have many API providers, therefore iterate over all of them
            foreach (var api in apisUsed)
            {
                using (var token = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    // Fetch from the API
                    var response = await api.Fetch(mediaType, token.Token);

                    // If the response is not null, we have a successful URL, return it
                    if (response.Success)
                        return response;
                }

                // Check if we've timed out before progressing to the next API
                cancellationToken.ThrowIfCancellationRequested();
            }

            // We got nothing, no cute animals at all
            return null;
        }
    }
}