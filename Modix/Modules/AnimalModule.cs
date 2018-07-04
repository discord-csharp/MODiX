using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Services.Animals;
using Serilog;

namespace Modix.Modules
{
    [Name("Animals"), Summary("Get pictures of cute animals!")]
    public class AnimalModule : ModuleBase
    {
        private const string Gif = "gif";
        private readonly IAnimalService _animalService;

        public AnimalModule(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        [Command("animal", RunMode = RunMode.Async), Alias("a"), Summary("Get an animal picture")]
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

            await GetAnimalAsync(animalType, mediaTypeRequested);
        }

        [Command("cat", RunMode = RunMode.Async), Alias("c"), Summary("Get a picture of a cat")]
        public async Task Cat(string mediaTypeRequested = null)
        {
            await GetAnimalAsync(AnimalType.Cat, mediaTypeRequested);
        }

        // I don't know if the fox api has gifs or not; therefore, pictures are only requested and returned.
        // Search logic for trying to find a gif has not been implemented for this api
        [Command("fox", RunMode = RunMode.Async), Alias("f"), Summary("Get a picture of a fox")]
        public async Task Fox()
        {
            await GetAnimalAsync(AnimalType.Fox);
        }

        private async Task GetAnimalAsync(AnimalType animal, string mediaTypeRequested = null)
        {
            var mediaType = !string.IsNullOrWhiteSpace(mediaTypeRequested) && mediaTypeRequested.Contains(Gif)
                ? MediaType.Gif
                : MediaType.Jpg;

            try
            {
                var reply = await _animalService.GetAsync(animal, mediaType);

                if (reply != null) await ProcessResponseAsync(mediaType, reply);
            }
            catch (TaskCanceledException)
            {
                await ReplyAsync($"Couldn't get a {animal.ToString().ToLower()} picture in time :(");
            }
            catch (Exception exc)
            {
                await ReplyAsync($"Couldn't get a {animal.ToString().ToLower()} picture: {exc.Message}");
            }
        }

        private async Task ProcessResponseAsync(MediaType type, Response animal)
        {
            switch (animal)
            {
                case ByteResponse byteResponse:
                    var fileName = "animal." + type.ToString().ToLower();

                    using (var stream = new MemoryStream(byteResponse.Bytes))
                    {
                        await Context.Channel.SendFileAsync(stream, fileName);
                    }

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