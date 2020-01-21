#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Extensions;
using Modix.Services.Utilities;
using Newtonsoft.Json;

namespace Modix.Services.IsUp
{
    public class IsUpCommand : IRequest
    {
        public IsUpCommand(IMessage message, string url)
        {
            Message = message;
            Url = url;
        }

        public IMessage Message { get; }
        public string Url { get; }
    }

    public class IsUpCommandHandler : AsyncRequestHandler<IsUpCommand>
    {
        private const string _apiBaseURl = "https://isitdown.site/api/v3/";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public IsUpCommandHandler(IHttpClientFactory httpClientFactory,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            _httpClientFactory = httpClientFactory;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        protected override async Task Handle(IsUpCommand request, CancellationToken cancellationToken)
        {
            var message = await request.Message.ReplyAsync("",
                new EmbedBuilder()
                .WithTitle($"Checking status of {request.Url}")
                .WithUserAsAuthor(request.Message.Author)
                .WithColor(Color.Orange)
                .Build());

            var apiQueryUrl = $"{_apiBaseURl}{request.Url}";

            if (Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                apiQueryUrl = $"{_apiBaseURl}{uri.Host}";
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(apiQueryUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await message.DeleteAsync();
                await request.Message.ReplyAsync($"Failed looking up {_apiBaseURl} - {response.ReasonPhrase}");
                return;
            }

            var jsonToDeserialize = await response.Content.ReadAsStringAsync();

            var isItUp = JsonConvert.DeserializeObject<IsUpResponseModel>(jsonToDeserialize);

            EmbedBuilder builder;

            if (isItUp.Host is null || isItUp.ResponseCode is null)
            {
                builder = new EmbedBuilder()
                    .WithTitle($"Host: { (isItUp.Host != null ? $"```{isItUp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(request.Message.Author)
                    .WithColor(Color.Red)
                    .WithDescription($"Something went wrong querying: `{request.Url}` Is that a valid URL?");
            }
            else
            {
                builder = new EmbedBuilder()
                    .WithTitle($"Host: { (isItUp.Host != null ? $"```{isItUp.Host}```" : "No Host found")}")
                    .WithUserAsAuthor(request.Message.Author)
                    .WithColor(Color.Green)
                    .AddField("Status ", $"{(isItUp.IsSiteDown ? "❌" : "✅")} {isItUp.ResponseCode}", true);
            }

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(request.Message.Author, builder,
                async (e) =>
            {
                await message.ModifyAsync(a =>
                {
                    a.Content = string.Empty;
                    a.Embed = e.Build();
                });
                return message;
            });

            await request.Message.DeleteAsync();
        }
    }
}
