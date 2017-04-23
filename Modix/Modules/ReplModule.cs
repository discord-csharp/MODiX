using System;
using System.Collections.Generic;
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
    }

    public class ReplModule : ModuleBase
    {
        private const string ReplRemoteUrl =
            "http://csharpdiscordfn.azurewebsites.net/api/EvalTrigger?code={0}";

        private readonly ModixConfig _config;

        private static readonly HttpClient _client = new HttpClient();

        public ReplModule(ModixConfig config)
        {
            _config = config;
        }

        [Command("exec", RunMode = RunMode.Async), Alias("eval"), Summary("Executes code!")]
        public async Task ReplInvoke([Remainder] string code)
        {
            var guildUser = Context.User as SocketGuildUser;
            var message = await Context.Channel.SendMessageAsync("Working...");
            var key = _config.ReplToken;
            
            string cleanCode = code.Replace("```csharp", string.Empty).Replace("```cs", string.Empty).Replace("```", string.Empty);
            cleanCode = Regex.Replace(cleanCode.Trim(), "^`|`$", string.Empty); //strip out the ` characters from the beginning and end of the string

            var content = new StringContent(cleanCode, Encoding.UTF8, "text/plain");
            
            HttpResponseMessage res;
            try
            {
                var tokenSrc = new CancellationTokenSource(15000);
                res = await _client.PostAsync(string.Format(ReplRemoteUrl, key), content, tokenSrc.Token);
            }
            catch(TaskCanceledException)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: Gave up waiting for a response from the REPL service."; });
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await message.ModifyAsync(a => { a.Content = $"Exec failed: {res.StatusCode}"; });
                return;
            }

            var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

            var embed = new EmbedBuilder()
               .WithTitle("Eval Result")
               .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Successful" : "Failed")
               .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? new Color(0, 255, 0) : new Color(255, 0, 0))
               .WithAuthor(a => a.WithIconUrl(Context.User.GetAvatarUrl()).WithName(guildUser?.Nickname ?? Context.User.Username))
               .WithFooter(a => a.WithText($"Compile: {parsedResult.CompileTime.TotalMilliseconds:F}ms | Execution: {parsedResult.ExecutionTime.TotalMilliseconds:F}ms"));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(cleanCode, "cs")));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnValue?.GetType()?.Name ?? "null"}")
                                     .WithValue(Format.Code($"{parsedResult.ReturnValue?.ToString() ?? " "}", "txt")));
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.ConsoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code(parsedResult.ConsoleOut, "txt")));
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.Exception))
            {
                var diffFormatted = Regex.Replace(parsedResult.Exception, "^", "- ", RegexOptions.Multiline);
                embed.AddField(a => a.WithName($"Exception: {parsedResult.ExceptionType}")
                                     .WithValue(Format.Code(diffFormatted, "diff")));
            }

            await message.ModifyAsync(a => {
                a.Content = string.Empty;
                a.Embed = embed.Build();
            });
        }
    }
}
