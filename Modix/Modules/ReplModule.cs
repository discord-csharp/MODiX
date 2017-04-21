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
            "http://csharpdiscordfn.azurewebsites.net/api/EvalTrigger?code=MGx5t5RZ6mmmy1jhVZ1amrWr8S3nPyCzG6bTR1mYbwULdhfuq86Ckg==";

        [Command("exec", RunMode = RunMode.Async), Alias("eval"), Summary("Executes code!")]
        public async Task ReplInvoke([Remainder] string code)
        {
            string cleanCode = code.Replace("```csharp", "").Replace("```cs", "").Replace("```", "");

            var client = new HttpClient();
            var res = await client.PostAsync(ReplRemoteUrl, new StringContent(cleanCode));

            if (res.StatusCode == HttpStatusCode.Forbidden)
            {
                await ReplyAsync("Exec failed: You used a forbidden class. <@144084036036329472>");
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await ReplyAsync("Exec failed: " + res.StatusCode);
                return;
            }

            var parsedResult = JsonConvert.DeserializeObject<Result>(await res.Content.ReadAsStringAsync());

            var embed = new EmbedBuilder()
               .WithTitle("Eval Result")
               .WithDescription(string.IsNullOrEmpty(parsedResult.Exception) ? "Successful" : $"Failed: {parsedResult.ExceptionType} - {parsedResult.Exception}")
               .WithColor(string.IsNullOrEmpty(parsedResult.Exception) ? new Color(0, 255, 0) : new Color(255, 0, 0))
               .WithAuthor(a => a.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl()).WithName(Context.Client.CurrentUser.Username))
               .WithFooter(a => a.WithText($"{(parsedResult.ExecutionTime.TotalMilliseconds + parsedResult.CompileTime.TotalMilliseconds):F} ms"));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(cleanCode, "cs")));

            if (parsedResult.ReturnValue != null)
            {
                embed.AddField(a => a.WithName($"Result: {parsedResult.ReturnValue?.GetType()?.Name ?? "null"}")
                                     .WithValue(Format.Code($"{parsedResult.ReturnValue?.ToString() ?? " "}", "txt")));
            }

            if (!string.IsNullOrWhiteSpace(parsedResult.ConsoleOut))
            {
                embed.AddField(a => a.WithName("Console Output")
                                     .WithValue(Format.Code($"{parsedResult.ConsoleOut}", "txt")));
            }
            
            await Context.Channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);
        }
    }
}
