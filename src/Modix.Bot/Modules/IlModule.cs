using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Modix.Bot.Responders.AutoRemoveMessages;
using Modix.Data.Models.Core;
using Modix.Services;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Modules
{
    [Name("Decompiler")]
    [Summary("Compile code & view the IL.")]
    [HelpTags("il")]
    public class IlModule : ModuleBase
    {
        private const string DefaultIlRemoteUrl = "http://csdiscord-repl-service:31337/Il";
        private readonly string _ilUrl;
        private readonly PasteService _pasteService;
        private readonly AutoRemoveMessageService _autoRemoveMessageService;
        private readonly IHttpClientFactory _httpClientFactory;

        public IlModule(
            PasteService pasteService,
            AutoRemoveMessageService autoRemoveMessageService,
            IHttpClientFactory httpClientFactory,
            IOptions<ModixConfig> modixConfig)
        {
            _pasteService = pasteService;
            _autoRemoveMessageService = autoRemoveMessageService;
            _httpClientFactory = httpClientFactory;
            _ilUrl = string.IsNullOrWhiteSpace(modixConfig.Value.IlUrl) ? DefaultIlRemoteUrl : modixConfig.Value.IlUrl;
        }

        [Command("il"), Alias("decompile"), Summary("Compile & return the resulting IL of C# code.")]
        public async Task ReplInvokeAsync(
            [Remainder]
            [Summary("The code to decompile.")]
                string code)
        {
            if (!(Context.Channel is IGuildChannel))
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

            var guildUser = Context.User as IGuildUser;
            var message = await Context.Channel.SendMessageAsync("Working...");

            var content = FormatUtilities.BuildContent(code);

            HttpResponseMessage res;
            try
            {
                var client = _httpClientFactory.CreateClient();

                using (var tokenSrc = new CancellationTokenSource(30000))
                {
                    res = await client.PostAsync(_ilUrl, content, tokenSrc.Token);
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

            var embed = await BuildEmbedAsync(guildUser, code, parsedResult);

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

        private async Task<EmbedBuilder> BuildEmbedAsync(IGuildUser guildUser, string code, string result)
        {
            var failed = result.Contains("Emit Failed");

            var embed = new EmbedBuilder()
               .WithTitle("Decompile Result")
               .WithDescription(result.Contains("Emit Failed") ? "Failed" : "Successful")
               .WithColor(failed ? new Color(255, 0, 0) : new Color(0, 255, 0))
               .WithUserAsAuthor(guildUser);

            embed.AddField(a => a.WithName("Code").WithValue(Format.Code(code, "cs")));

            const int MAX_LENGTH = 990;

            embed.AddField(a => a.WithName($"Result:")
                 .WithValue(Format.Code(result.TruncateTo(MAX_LENGTH), "asm")));

            await embed.UploadToServiceIfBiggerThan(result, MAX_LENGTH, _pasteService);

            return embed;
        }
    }
}
