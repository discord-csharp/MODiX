using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Bot.Modules
{
    [Name("Repl")]
    [Summary("Execute & demonstrate code snippets.")]
    [HelpTags("eval", "exec")]
    public partial class ReplModule : ModuleBase
    {
        private const int MaxFormattedFieldSize = 1000;
        private const string DefaultReplRemoteUrl = "http://csdiscord-repl-service:31337/Eval";
        private readonly string _replUrl;
        private readonly CodePasteService _pasteService;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReplModule(
            CodePasteService pasteService,
            IAutoRemoveMessageService autoRemoveMessageService,
            IHttpClientFactory httpClientFactory,
            IOptions<ModixConfig> modixConfig)
        {
            _pasteService = pasteService;
            _autoRemoveMessageService = autoRemoveMessageService;
            _httpClientFactory = httpClientFactory;
            _pasteService = pasteService;
            _replUrl = string.IsNullOrWhiteSpace(modixConfig.Value.ReplUrl) ? DefaultReplRemoteUrl : modixConfig.Value.ReplUrl;
        }

        [Command("exec"), Alias("eval", "e"), Summary("Executes the given C# code and returns the result.")]
        public async Task ReplInvokeAsync(
            [Remainder]
            [Summary("The code to execute.")]
                string code)
        {
            if (Context.Channel is not IGuildChannel || Context.User is not IGuildUser guildUser)
            {
                await ModifyOrSendErrorEmbedAsync("The REPL can only be executed in public guild channels.");
                return;
            }

            var message = await Context.Channel
                .SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("REPL Executing")
                    .WithUserAsAuthor(Context.User)
                    .WithColor(Color.LightOrange)
                    .WithDescription($"Compiling and Executing [your code]({Context.Message.GetJumpUrl()})...")
                    .Build());

            var content = FormatUtilities.BuildContent(code);

            // make it easier to trace calls back to discord if moderation or investigation needs to happen
            content.Headers.TryAddWithoutValidation("X-Modix-DiscordUserId", Context.User.Id.ToString());
            var messageLink = $"https://discord.com/channels/{Context.Guild.Id}/{Context.Channel.Id}/{message.Id}";
            content.Headers.TryAddWithoutValidation("X-Modix-MessageLink", messageLink);

            HttpResponseMessage res;
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientNames.RetryOnTransientErrorPolicy);
                res = await client.PostAsync(_replUrl, content);
            }
            catch (IOException ex)
            {
                await ModifyOrSendErrorEmbedAsync("Received an invalid response from the REPL service." +
                                             $"\n\n{Format.Bold("Details:")}\n{ex.Message}", message);
                return;
            }
            catch (Exception ex)
            {
                await ModifyOrSendErrorEmbedAsync("An error occurred while sending a request to the REPL service. " +
                                             "This may be due to a StackOverflowException or exceeding the 30 second timeout." +
                                             $"\n\n{Format.Bold("Details:")}\n{ex.Message}", message);
                return;
            }

            if (!res.IsSuccessStatusCode && res.StatusCode != HttpStatusCode.BadRequest)
            {
                await ModifyOrSendErrorEmbedAsync($"Status Code: {(int)res.StatusCode} {res.StatusCode}", message);
                return;
            }

            var parsedResult = JsonSerializer.Deserialize(await res.Content.ReadAsStringAsync(), JsonMetadataContext.Default.ReplResult);

            var embed = await BuildEmbedAsync(guildUser, parsedResult);

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.User, embed, async (e) =>
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

        private async Task ModifyOrSendErrorEmbedAsync(string error, IUserMessage message = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle("REPL Error")
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.Red)
                .AddField("Tried to execute", $"[this code]({Context.Message.GetJumpUrl()})")
                .WithDescription(error);

            if (message == null)
            {
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await message.ModifyAsync(msg =>
                {
                    msg.Content = null;
                    msg.Embed = embed.Build();
                });
            }
        }

        private async Task<EmbedBuilder> BuildEmbedAsync(IGuildUser guildUser, ReplResult parsedResult)
        {
            var returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
            var consoleOut = parsedResult.ConsoleOut;
            var hasException = !string.IsNullOrEmpty(parsedResult.Exception);
            var status = hasException ? "Failure" : "Success";

            var embed = new EmbedBuilder()
                    .WithTitle($"REPL Result: {status}")
                    .WithColor(hasException ? Color.Red : Color.Green)
                    .WithUserAsAuthor(guildUser)
                    .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"));

            embed.WithDescription(FormatOrEmptyCodeblock(parsedResult.Code, "cs"));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnTypeName}".TruncateTo(EmbedFieldBuilder.MaxFieldNameLength))
                                     .WithValue(FormatOrEmptyCodeblock(returnValue.TruncateTo(MaxFormattedFieldSize), "json")));
                await embed.UploadToServiceIfBiggerThanAsync(returnValue, MaxFormattedFieldSize, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(consoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(consoleOut.TruncateTo(MaxFormattedFieldSize), "txt")));
                await embed.UploadToServiceIfBiggerThanAsync(consoleOut, MaxFormattedFieldSize, _pasteService);
            }

            if (hasException)
            {
                var diffFormatted = ExceptionStartRegex().Replace(parsedResult.Exception, "- ");
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}".TruncateTo(EmbedFieldBuilder.MaxFieldNameLength))
                                     .WithValue(Format.Code(diffFormatted.TruncateTo(MaxFormattedFieldSize), "diff")));
                await embed.UploadToServiceIfBiggerThanAsync(diffFormatted, MaxFormattedFieldSize, _pasteService);
            }

            return embed;
        }

        private static string FormatOrEmptyCodeblock(string input, string language)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "```\n```";

            return Format.Code(input, language);
        }

        [GeneratedRegex("^", RegexOptions.Multiline)]
        private static partial Regex ExceptionStartRegex();
    }

    public class ReplResult
    {
        public object ReturnValue { get; set; }
        public string Exception { get; set; }
        public string Code { get; set; }
        public string ExceptionType { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public TimeSpan CompileTime { get; set; }
        public string ConsoleOut { get; set; }
        public string ReturnTypeName { get; set; }
    }
}
