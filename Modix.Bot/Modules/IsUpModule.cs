#nullable enable
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.Extensions;
using Modix.Services.IsUp;
using Modix.Services.Utilities;
using Newtonsoft.Json;

namespace Modix.Bot.Modules
{
    [Name("Isup")]
    [Summary("Detects if a website has an outage")]
    [HelpTags("isup")]
    public class IsUpModule : ModuleBase
    {
        private const string _apiBaseURl = "https://isitdown.site/api/v3/";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public IsUpModule(IHttpClientFactory httpClientFactory, IAutoRemoveMessageService autoRemoveMessageService)
        {
            _httpClientFactory = httpClientFactory;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [Command("isup")]
        public async Task IsUp([Summary("Url to ping")] string url)
        {
            var message = await Context.Message.ReplyAsync("",
                new EmbedBuilder()
                .WithTitle($"Checking status of {url}")
                .WithUserAsAuthor(Context.Message.Author)
                .WithColor(Color.Orange)
                .Build());

            var apiQueryUrl = $"{_apiBaseURl}{url}";

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                apiQueryUrl = $"{_apiBaseURl}{uri.Host}";
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(apiQueryUrl);

            if (!response.IsSuccessStatusCode)
            {
                await message.DeleteAsync();
                await Context.Message.ReplyAsync($"Failed looking up {_apiBaseURl} - {response.ReasonPhrase}");
                return;
            }

            var jsonToDeserialize = await response.Content.ReadAsStringAsync();

            var isItUp = JsonConvert.DeserializeObject<IsUpResponseModel>(jsonToDeserialize);

            EmbedBuilder builder;

            if (isItUp.Host is null || isItUp.ResponseCode is null)
            {
                builder = new EmbedBuilder()
                    .WithTitle($"Host: { (isItUp.Host != null ? $"```{isItUp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(Context.Message.Author)
                    .WithColor(Color.Red)
                    .WithDescription($"Something went wrong querying: `{url}` Is that a valid URL?");
            }
            else
            {
                builder = new EmbedBuilder()
                    .WithTitle($"Host: { (isItUp.Host != null ? $"```{isItUp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(Context.Message.Author)
                    .WithColor(Color.Green)
                    .AddField("Status ", $"{(isItUp.IsSiteDown ? "❌" : "✅")} {isItUp.ResponseCode}", true);
            }

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.Message.Author, builder,
                async (e) =>
            {
                await message.ModifyAsync(a =>
                {
                    a.Content = string.Empty;
                    a.Embed = e.Build();
                });
                return message;
            });

            await Context.Message.DeleteAsync();
        }
    }
}
