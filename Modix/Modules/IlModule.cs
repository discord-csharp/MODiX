using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Data.Models.Core;
using System.Threading;
using Discord.WebSocket;
using Serilog;
using Modix.Services.AutoCodePaste;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Decompiler"), Summary("Compile code & view the IL")]
    public class IlModule : ModuleBase
    {
        private const string ReplRemoteUrl = "http://CSDiscord/Il";
        private readonly CodePasteService _pasteService;

        private static readonly HttpClient _client = new HttpClient();

        public IlModule(ModixConfig config, CodePasteService pasteService)
        {
            _pasteService = pasteService;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", config.ReplToken);
        }

        [Command("il", RunMode = RunMode.Async), Summary("Compile & return the resulting IL of C# code")]
        public async Task ReplInvoke([Remainder] string code)
        {
            if (!(Context.Channel is SocketGuildChannel))
            {
                await ReplyAsync("il can only be executed in public guild channels.");
                return;
            }
            code = FormatUtilities.StripFormatting(code);
            if (code.Length > 1000)
            {
                await ReplyAsync("Decompile Failed: Code is greater than 1000 characters in length");
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
                await message.ModifyAsync(a => { a.Content = $"Gave up waiting for a response from the Decompile service."; });
                return;
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(a => { a.Content = $"Decompile failed: {ex.Message}"; });
                Log.Error(ex, "Decompile Failed");
                return;
            }

            if (!res.IsSuccessStatusCode & res.StatusCode != HttpStatusCode.BadRequest)
            {
                await message.ModifyAsync(a => { a.Content = $"Decompile failed: {res.StatusCode}"; });
                return;
            }

            var parsedResult = await res.Content.ReadAsStringAsync();

            var embed = await BuildEmbed(guildUser, code, parsedResult);

            await message.ModifyAsync(a =>
            {
                a.Content = string.Empty;
                a.Embed = embed.Build();
            });

            await Context.Message.DeleteAsync();
        }

        private async Task<EmbedBuilder> BuildEmbed(SocketGuildUser guildUser, string code, string result)
        {
            var failed = result.Contains("Emit Failed");

            var embed = new EmbedBuilder()
               .WithTitle("Decompile Result")
               .WithDescription(result.Contains("Emit Failed") ? "Failed" : "Successful")
               .WithColor(failed ? new Color(255, 0, 0) : new Color(0, 255, 0))
               .WithAuthor(a => a.WithIconUrl(Context.User.GetAvatarUrl()).WithName(guildUser?.Nickname ?? Context.User.Username));

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(code, "cs")));

            embed.AddField(a => a.WithName($"Result:")
                 .WithValue(Format.Code(result.TruncateTo(990), "asm")));

            await embed.UploadToServiceIfBiggerThan(result, "asm", 990, _pasteService);

            return embed;
        }
    }
}
