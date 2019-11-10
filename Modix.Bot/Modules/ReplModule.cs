using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using Newtonsoft.Json;

namespace Modix.Modules
{
    public class Result
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

    [Name("Repl")]
    [Summary("Execute & demonstrate code snippets.")]
    [HelpTags("eval", "exec")]
    public class ReplModule : ModuleBase
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

        [Command("exec"), Alias("eval"), Summary("Executes the given C# code and returns the result.")]
        public async Task ReplInvokeAsync(
            [Remainder]
            [Summary("The code to execute.")]
                string code)
        {
            if (!(Context.Channel is IGuildChannel) || !(Context.User is IGuildUser guildUser))
            {
                await ModifyOrSendErrorEmbed("The REPL can only be executed in public guild channels.");
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

            HttpResponseMessage res;
            try
            {
                var client = _httpClientFactory.CreateClient();
                res = await client.PostAsync(_replUrl, content);

            }
            catch (IOException ex)
            {
                await ModifyOrSendErrorEmbed("Recieved an invalid response from the REPL service." +
                                             $"\n\n{Format.Bold("Details:")}\n{ex.Message}", message);
                return;
            }
            catch (Exception ex)
            {
                await ModifyOrSendErrorEmbed("An error occurred while sending a request to the REPL service. " +
                                             "This is probably due to container exhaustion - try again later." +
                                             $"\n\n{Format.Bold("Details:")}\n{ex.Message}", message);
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await ModifyOrSendErrorEmbed($"Status Code: {(int)res.StatusCode} {res.StatusCode}", message);
                return;
            }

            var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

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

        private async Task ModifyOrSendErrorEmbed(string error, IUserMessage message = null)
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

        private async Task<EmbedBuilder> BuildEmbedAsync(IGuildUser guildUser, Result parsedResult)
        {
            var returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
            var consoleOut = parsedResult.ConsoleOut;

            var embed = new EmbedBuilder()
                .WithTitle("REPL Result")
                .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Success" : "Failure")
                .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? Color.Green : Color.Red)
                .WithUserAsAuthor(guildUser)
                .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"));

            var code = new string(parsedResult.Code.Take(MaxFormattedFieldSize).ToArray());
            embed.AddField(a => a.WithName("Code").WithIsInline(false).WithValue(Format.Code(code, "cs")));

            if (parsedResult.Code.Length > MaxFormattedFieldSize)
            {
                var remaining = new string(parsedResult.Code.Skip(MaxFormattedFieldSize).ToArray());
                embed.AddField(a => a.WithIsInline(false).WithName("Continued...").WithValue(Format.Code(remaining, "cs")));
            }

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnTypeName ?? "null"}")
                                     .WithValue(Format.Code($"{returnValue.TruncateTo(MaxFormattedFieldSize)}", "json")));
                await embed.UploadToServiceIfBiggerThan(returnValue, "json", MaxFormattedFieldSize, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(consoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(consoleOut.TruncateTo(MaxFormattedFieldSize), "txt")));
                await embed.UploadToServiceIfBiggerThan(consoleOut, "txt", MaxFormattedFieldSize, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.Exception))
            {
                var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
                                     .WithValue(Format.Code(diffFormatted.TruncateTo(MaxFormattedFieldSize), "diff")));
                await embed.UploadToServiceIfBiggerThan(diffFormatted, "diff", MaxFormattedFieldSize, _pasteService);
            }

            return embed;
        }
    }
}
