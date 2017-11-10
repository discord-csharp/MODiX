using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Modix.Data.Models;
using System.Threading;
using Discord.WebSocket;
using Modix.Utilities;

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

    public class ReplModule : ModuleBase
    {
        private const string ReplRemoteUrl = "http://CSDiscord/Eval";

        private readonly ModixConfig _config;

        private static readonly HttpClient _client = new HttpClient();

        public ReplModule(ModixConfig config)
        {
            _config = config;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config.ReplToken);
        }

        [Command("exec", RunMode = RunMode.Async), Alias("eval"), Summary("Executes code!")]
        public async Task ReplInvoke([Remainder] string code)
        {
            if (!(Context.Channel is SocketGuildChannel))
            {
                await ReplyAsync("exec can only be executed in public guild channels.");
                return;
            }

            if (code.Length > 1024)
            {
                await ReplyAsync("Exec failed: Code is greater than 1024 characters in length");
                return;
            }

            var guildUser = Context.User as SocketGuildUser;
            var message = await Context.Channel.SendMessageAsync("Working...");

            var content = FormatUtilities.BuildContent(code);

            HttpResponseMessage res;
            try
            {
                var tokenSrc = new CancellationTokenSource(30000);
                res = await _client.PostAsync(ReplRemoteUrl, content, tokenSrc.Token);
            }
            catch (TaskCanceledException)
            {
                await message.ModifyAsync(a => { a.Content = $"Gave up waiting for a response from the REPL service."; });
                return;
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: {ex.Message}"; });
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: {res.StatusCode}"; });
                return;
            }

            var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

            var embed = BuildEmbed(guildUser, parsedResult);

            await message.ModifyAsync(a =>
            {
                a.Content = string.Empty;
                a.Embed = embed.Build();
            });

            await Context.Message.DeleteAsync();
        }

        private EmbedBuilder BuildEmbed(SocketGuildUser guildUser, Result parsedResult)
        {
            var returnValue = TrimIfNeeded(parsedResult.ReturnValue?.ToString() ?? " ", 1000);
            var consoleOut = TrimIfNeeded(parsedResult.ConsoleOut, 1000);
            var exception = TrimIfNeeded(parsedResult.Exception ?? string.Empty, 1000);

            var embed = new EmbedBuilder()
               .WithTitle("Eval Result")
               .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Successful" : "Failed")
               .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? new Color(0, 255, 0) : new Color(255, 0, 0))
               .WithAuthor(a => a.WithIconUrl(Context.User.GetAvatarUrl()).WithName(guildUser?.Nickname ?? Context.User.Username))
               .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(parsedResult.Code, "cs")));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnTypeName ?? "null"}")
                                     .WithValue(Format.Code($"{returnValue}", "txt")));
            }

            if (!string.IsNullOrWhiteSpace(consoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(consoleOut, "txt")));
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.Exception))
            {
                var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
                                     .WithValue(Format.Code(diffFormatted, "diff")));
            }

            return embed;
        }

        private static string TrimIfNeeded(string value, int len)
        {
            if (value.Length > len)
            {
                return value.Substring(0, len);
            }

            return value;
        }
    }
}
