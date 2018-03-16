namespace Modix.Modules
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Modix.Services.Animals;
    using Serilog;

    public enum MediaType
    {
        Jpg,
        Gif
    }

    public enum AnimalType
    {
        Cat,
        Fox,
        Dog
    }

    [Summary("Cute animals!")]
    public class AnimalModule : ModuleBase
    {
        private readonly IAnimalService _animalService;
        private const string Gif = "gif";

        public AnimalModule(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        [Command("animal", RunMode = RunMode.Async), Alias("a")]
        public async Task Animal(string animalRequested = null, string mediaTypeRequested = null)
        {
            /* There is one small bug.
             If an unknown animal or media type is passed as the "animal requested" argument.
             (i.e. invoking the command with out of order parameters: 

             "!animal gif cat" <- wrong order
             "!animal cat gif" <- right order

             The command will fail to recognize the animal and display a cat picture
            */

            Log.Information($"Animal requested: {animalRequested}");
            Log.Information($"Media Type Requested: {mediaTypeRequested}");

            Enum.TryParse(animalRequested, true, out AnimalType animalType);

            await GetAnimal(animalType, mediaTypeRequested);
        }

        [Command("cat", RunMode = RunMode.Async), Alias("c")]
        public async Task Cat(string mediaTypeRequested = null) => await GetAnimal(AnimalType.Cat, mediaTypeRequested);

        // I don't know if the fox api has gifs or not; therefore, pictures are only requested and returned.
        // Search logic for trying to find a gif has not been implemented for this api
        [Command("fox", RunMode = RunMode.Async), Alias("f")]
        public async Task Fox() => await GetAnimal(AnimalType.Fox);

        private async Task GetAnimal(AnimalType animal, string mediaTypeRequested = null)
        {
            var mediaType = !string.IsNullOrWhiteSpace(mediaTypeRequested) && mediaTypeRequested.Contains(Gif)
                ? MediaType.Gif
                : MediaType.Jpg;

            try
            {
                var reply = await _animalService.Get(animal, mediaType);

                if (reply != null)
                {
                    await ProcessResponse(mediaType, reply);
                }
            }
            catch (TaskCanceledException)
            {
                await ReplyAsync("Couldn't get a fox picture in time :(");
            }
            catch (Exception exc)
            {
                await ReplyAsync($"Couldn't get a fox picture: {exc.Message}");
            }
        }

        private async Task ProcessResponse(MediaType type, Response animal)
        {
            switch (animal)
            {
                case ByteResponse byteResponse:
                    var fileName = "animal." + type.ToString().ToLower();

                    using (var stream = new MemoryStream(byteResponse.Bytes))
                        await Context.Channel.SendFileAsync(stream, fileName);
                    break;

                case UrlResponse urlResponse:
                    await ReplyAsync(urlResponse.Url);
                    break;

                default:
                    await ReplyAsync("Something went wrong while finding a cute animal");
                    break;
            }
        }
    }
}