using System;
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
using Modix.Services.AutoCodePaste;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Utilities;
using Newtonsoft.Json;
using Serilog;

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

    [Name("Repl"), Summary("Execute & demonstrate code snippets")]
    public class ReplModule : ModuleBase
    {
        private const string ReplRemoteUrl = "http://csdiscord-repl-service:31337/Eval";
        private readonly CodePasteService _pasteService;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReplModule(
            CodePasteService pasteService,
            IAutoRemoveMessageService autoRemoveMessageService,
            IHttpClientFactory httpClientFactory)
        {
            _pasteService = pasteService;
            _autoRemoveMessageService = autoRemoveMessageService;
            _httpClientFactory = httpClientFactory;
            _pasteService = pasteService;
        }

        [Command("exec"), Alias("eval"), Summary("Executes the given C# code and returns the result")]
        public async Task ReplInvoke([Remainder] string code)
        {
            if (!(Context.Channel is SocketGuildChannel))
            {
                await ReplyAsync("exec can only be executed in public guild channels.");
                return;
            }

            if (code.Length > 1000)
            {
                await ReplyAsync("Exec failed: Code is greater than 1000 characters in length");
                return;
            }

            var guildUser = Context.User as SocketGuildUser;
            var message = await Context.Channel.SendMessageAsync("Working...");

            var content = FormatUtilities.BuildContent(code);

            HttpResponseMessage res;
            try
            {
                var client = _httpClientFactory.CreateClient(nameof(ReplModule));

                using (var tokenSrc = new CancellationTokenSource(30000))
                {
                    res = await _httpClientFactory.CreateClient().PostAsync(ReplRemoteUrl, content, tokenSrc.Token);
                }
            }
            catch (TaskCanceledException)
            {
                await message.ModifyAsync(a => { a.Content = $"Gave up waiting for a response from the REPL service."; });
                return;
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: {ex.Message}"; });
                Log.Error(ex, "Exec Failed");
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: {res.StatusCode}"; });
                return;
            }

            var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

            var embed = await BuildEmbed(guildUser, parsedResult);

            await message.ModifyAsync(a =>
            {
                a.Content = string.Empty;
                a.Embed = embed.Build();
            });

            await Context.Message.DeleteAsync();

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(message, Context.User);
        }

        private async Task<EmbedBuilder> BuildEmbed(SocketGuildUser guildUser, Result parsedResult)
        {
            var returnValue = parsedResult.ReturnValue?.ToString() ?? " ";
            var consoleOut = parsedResult.ConsoleOut;

            var embed = new EmbedBuilder()
                .WithTitle("Eval Result")
                .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Successful" : "Failed")
                .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? new Color(0, 255, 0) : new Color(255, 0, 0))
                .WithAuthor(a => a.WithIconUrl(Context.User.GetAvatarUrl()).WithName(guildUser?.Nickname ?? Context.User.Username))
                .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"
                    + " | React with ❌ to remove this embed."));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(parsedResult.Code, "cs")));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnTypeName ?? "null"}")
                                     .WithValue(Format.Code($"{returnValue.TruncateTo(1000)}", "json")));
                await embed.UploadToServiceIfBiggerThan(returnValue, "json", 1000, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(consoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(consoleOut.TruncateTo(1000), "txt")));
                await embed.UploadToServiceIfBiggerThan(consoleOut, "txt", 1000, _pasteService);
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.Exception))
            {
                var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
                                     .WithValue(Format.Code(diffFormatted.TruncateTo(1000), "diff")));
                await embed.UploadToServiceIfBiggerThan(diffFormatted, "diff", 1000, _pasteService);
            }

            return embed;
        }
    }
}
