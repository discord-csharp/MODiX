#nullable enable
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
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

        [SlashCommand("godbolt", "Compile and disassemble the JIT code.")]
        public async Task DisasmAsync(
            [Summary(description: "The code to compiler.")] string code,
            [Summary(description: "The language of code.")] Language language = Language.CSharp,
            [Summary(description: "Arguments to pass to the compiler.")] string arguments = "",
            [Summary(description: "Execute the code.")] bool execute = false)
        {
            var langId = language switch
            {
                Language.CSharp => "csharp",
                Language.FSharp => "fsharp",
                Language.VisualBasic => "vb",
                Language.IL => "il",
                _ => throw new ArgumentOutOfRangeException(nameof(language))
            };

            try
            {
                var response = $"""
                    ```asm
                    {await _godboltService.CompileAsync(code, langId, arguments, execute)}
                    ```
                    """;

                if (response.Length > DiscordConfig.MaxMessageSize)
                {
                    await FollowupAsync("Error: The disassembly code is too long to be converted to a message.");
                    return;
                }

                await FollowupAsync(text: response);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Error: {ex}");
                return;
            }
        }

        public enum Language
        {
            [ChoiceDisplay("C#")]
            CSharp,
            [ChoiceDisplay("F#")]
            FSharp,
            [ChoiceDisplay("VB.NET")]
            VisualBasic,
            [ChoiceDisplay("IL")]
            IL
        }
    }
}
