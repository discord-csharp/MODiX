using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Services.AutoCodePaste;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Utilities;
using Serilog;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;
using System.Net.Http.Headers;

namespace Modix.Modules
{
    [Name("Decompiler"), Summary("Compile code & view the IL")]
    public class IlModule : ModuleBase
    {
        private const string ReplRemoteUrl = "http://csdiscord-repl-service:31337/Il";
        private readonly CodePasteService _pasteService;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ModixConfig _config;

        public IlModule(
            CodePasteService pasteService,
            IAutoRemoveMessageService autoRemoveMessageService,
            IHttpClientFactory httpClientFactory,
            IOptions<ModixConfig> config)
        {
            _pasteService = pasteService;
            _autoRemoveMessageService = autoRemoveMessageService;
            _httpClientFactory = httpClientFactory;
            _config = config.Value;
        }

        [Command("il", RunMode = RunMode.Sync), Summary("Compile & return the resulting IL of C# code")]
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
                var client = _httpClientFactory.CreateClient("ReplClient");

                using (var tokenSrc = new CancellationTokenSource(30000))
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, ReplRemoteUrl)
                    {
                        Content = content
                    };

                    req.Headers.Authorization = new AuthenticationHeaderValue("Token", _config.ReplToken);

                    res = await client.SendAsync(req);
                }
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

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(message, Context.User);
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

            embed.WithFooter("React with ❌ to remove this embed.");

            return embed;
        }
    }
}
