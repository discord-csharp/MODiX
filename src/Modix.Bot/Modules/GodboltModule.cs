#nullable enable
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Modix.Bot.Attributes;
using Modix.Services.CommandHelp;
using Modix.Services.Godbolt;

namespace Modix.Bot.Modules
{
    [ModuleHelp("Godbolt", "Commands for working with Godbolt.")]
    public class GodboltModule : InteractionModuleBase
    {
        private readonly GodboltService _godboltService;

        public GodboltModule(GodboltService godboltService)
        {
            _godboltService = godboltService;
        }

        [SlashCommand("godbolt", description: "Compile and disassemble JIT code.")]
        [DoNotDefer]
        public Task DisasmAsync() => Context.Interaction.RespondWithModalAsync<GodboltDisasmModal>("godbolt_disasm");

        [ModalInteraction("godbolt_disasm")]
        public async Task ProcessDisasmAsync(GodboltDisasmModal modal)
        {
            try
            {
                var response = $"""
                    ```{modal.Language}
                    {modal.Code}
                    ```
                    Disassembly {(string.IsNullOrWhiteSpace(modal.Arguments) ? "" : $"with ``{modal.Arguments}``")}:
                    ```asm
                    {await _godboltService.CompileAsync(modal.Code, modal.Language, modal.Arguments)}
                    ```
                    """;

                if (response.Length > DiscordConfig.MaxMessageSize)
                {
                    await FollowupAsync("Error: The code is too long to be converted to a message.", allowedMentions: AllowedMentions.None);
                    return;
                }

                await FollowupAsync(response, allowedMentions: AllowedMentions.None);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Error: {ex}", allowedMentions: AllowedMentions.None);
                return;
            }
        }
    }
}
