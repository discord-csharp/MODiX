using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Modix.Data.Models.Core;
using Modix.RemoraShim.Errors;
using Modix.RemoraShim.Utilities;
using Modix.Services.CodePaste;
using Modix.Services.Utilities;

using Newtonsoft.Json;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Commands
{
    internal record ReplResult(
        object ReturnValue,
        string Exception,
        string Code,
        string ExceptionType,
        TimeSpan ExecutionTime,
        TimeSpan CompileTime,
        string ConsoleOut,
        string ReturnTypeName);

    public class ReplCommands
        : CommandGroup
    {
        public ReplCommands(
            ICommandContext context,
            CodePasteService pasteService,
            IDiscordRestChannelAPI channelApi,
            IHttpClientFactory httpClientFactory,
            IOptions<ModixConfig> modixConfig)
        {
            _context = context;
            _pasteService = pasteService;
            _channelApi = channelApi;
            _httpClientFactory = httpClientFactory;
            _replUrl = string.IsNullOrWhiteSpace(modixConfig.Value.ReplUrl)
                ? DefaultReplRemoteUrl
                : modixConfig.Value.ReplUrl;
        }

        [Command("exec", "eval", "e")]
        public async Task<Result> EvalAsync([Greedy] string code)
        {
            var messageContext = _context as MessageContext;

            var messageLink = $"https://discord.com/channels/{_context.GuildID.Value}/{_context.ChannelID}/{messageContext?.MessageID}";
            var username = $"{_context.User.Username}#{_context.User.Discriminator}";
            var avatarUrl = UserUtility.GetAvatarUrl(_context.User);

            var executingEmbed = new Embed(
                Title: "REPL EXECUTING",
                Author: new EmbedAuthor(
                    Name: username,
                    IconUrl: avatarUrl),
                Colour: Color.Orange,
                Description: $"Compiling and executing [your code]({messageLink})...");

            var executingMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, content: "", embeds: new[] { executingEmbed });
            if (!executingMessageResult.IsSuccess)
                return Result.FromError(executingMessageResult);

            var message = executingMessageResult.Entity;

            var content = FormatUtilities.BuildContent(code);
            content.Headers.TryAddWithoutValidation("X-Modix-DiscordUserId", _context.User.ID.ToString());
            content.Headers.TryAddWithoutValidation("X-Modix-DiscordUsername", username);
            content.Headers.TryAddWithoutValidation("X-Modix-MessageLink", messageLink);

            HttpResponseMessage responseMessage;

            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientNames.RetryOnTransientErrorPolicy);
                responseMessage = await httpClient.PostAsync(_replUrl, content);
            }
            catch (IOException ex)
            {
                var errorMessage = "Received an invalid response from the REPL service.\n\n"
                    + $"**Details:**\n{ex.Message}";

                await ModifyErrorEmbedAsync(errorMessage, username, avatarUrl, messageLink, message.ID);
                return Result.FromError(new ExceptionError(ex));
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred while sending a request to the REPL service. "
                    + "This may be due to a StackOverflowException or exceeding the 30 second timeout.\n\n"
                    + $"**Details:**\n{ex.Message}";

                await ModifyErrorEmbedAsync(errorMessage, username, avatarUrl, messageLink, message.ID);
                return Result.FromError(new ExceptionError(ex));
            }

            if (!responseMessage.IsSuccessStatusCode && responseMessage.StatusCode != HttpStatusCode.BadRequest)
            {
                var errorMessage = $"Status Code: {(int)responseMessage.StatusCode} {responseMessage.StatusCode}";

                await ModifyErrorEmbedAsync(errorMessage, username, avatarUrl, messageLink, message.ID);
                return Result.FromError(new HttpError(responseMessage.StatusCode));
            }

            var response = await responseMessage.Content.ReadAsStringAsync();
            var parsedResult = JsonConvert.DeserializeObject<ReplResult>(response);

            var resultEmbed = await BuildEmbedAsync(username, avatarUrl, parsedResult);

            var resultEmbedResult = await _channelApi.EditMessageAsync(_context.ChannelID, message.ID, content: null, embeds: new[] { resultEmbed });
            if (!resultEmbedResult.IsSuccess)
                return Result.FromError(resultEmbedResult);

            if (messageContext is not null)
                await _channelApi.DeleteMessageAsync(messageContext.ChannelID, messageContext.MessageID);

            return Result.FromSuccess();
        }

        private async Task<Embed> BuildEmbedAsync(string username, string avatarUrl, ReplResult replResult)
        {
            var returnValue = replResult.ReturnValue?.ToString() ?? " ";
            var consoleOut = replResult.ConsoleOut;
            var (status, color) = string.IsNullOrEmpty(replResult.Exception)
                ? ("Success", Color.Green)
                : ("Failure", Color.Red);

            var fields = new List<EmbedField>();

            if (replResult.ReturnValue is not null)
                await TruncateAndAddFieldAsync($"Result: {replResult.ReturnTypeName ?? "null"}", returnValue, "json");

            if (!string.IsNullOrWhiteSpace(consoleOut))
                await TruncateAndAddFieldAsync("Console Output", consoleOut, "txt");

            if (!string.IsNullOrWhiteSpace(replResult.Exception))
            {
                var diffFormatted = _diffFormattingRegex.Replace(replResult.Exception, "- ");
                await TruncateAndAddFieldAsync($"Exception: {replResult.ExceptionType}", diffFormatted, "diff");
            }

            return new Embed(
                Title: $"REPL Result: {status}",
                Colour: color,
                Author: new EmbedAuthor(
                    Name: username,
                    IconUrl: avatarUrl),
                Footer: new EmbedFooter(
                    Text: $"Compile: {replResult.CompileTime.TotalMilliseconds:F}ms | Execution: {replResult.ExecutionTime.TotalMilliseconds:F}ms"),
                Description: $"```cs\n{replResult.Code}\n```",
                Fields: fields);

            async Task TruncateAndAddFieldAsync(string name, string value, string language)
            {
                var truncatedValue = value.TruncateTo(MaxFormattedFieldSize);

                fields.Add(new EmbedField(
                    Name: name,
                    Value: string.IsNullOrWhiteSpace(value)
                        ? truncatedValue
                        : $"```{language}\n{truncatedValue}\n```"));

                if (truncatedValue.Length < value.Length)
                {
                    try
                    {
                        var uploadLink = await _pasteService.UploadCodeAsync(value, language);
                        fields.Add(new EmbedField(
                            Name: "More...",
                            Value: $"[View on Hastebin]({uploadLink})"));
                    }
                    catch (WebException ex)
                    {
                        fields.Add(new EmbedField(
                            Name: "More...",
                            Value: ex.Message));
                    }
                }
            }
        }

        private async Task ModifyErrorEmbedAsync(string error, string username, string avatarUrl, string commandMessageLink, Snowflake messageId)
        {
            var embed = new Embed(
                Title: "REPL Error",
                Author: new EmbedAuthor(
                    Name: username,
                    IconUrl: avatarUrl),
                Colour: Color.Red,
                Fields: new[] {
                    new EmbedField(
                        Name: "Tried to execute",
                        Value: $"[this code]({commandMessageLink})")},
                Description: error);

            await _channelApi.EditMessageAsync(_context.ChannelID, messageId, content: null, embeds: new[] { embed });
        }

        private const int MaxFormattedFieldSize = 1000;
        private const string DefaultReplRemoteUrl = "http://csdiscord-repl-service:31337/Eval";

        private readonly string _replUrl;
        private readonly ICommandContext _context;
        private readonly CodePasteService _pasteService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly Regex _diffFormattingRegex = new("^", RegexOptions.Multiline | RegexOptions.Compiled);
    }
}
