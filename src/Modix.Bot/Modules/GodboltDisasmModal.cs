#nullable enable
using Discord;
using Discord.Interactions;

namespace Modix.Bot.Modules
{
    public class GodboltDisasmModal : IModal
    {
        public string Title => "Godbolt";

        [InputLabel("Code")]
        [ModalTextInput("code", TextInputStyle.Paragraph, placeholder: "System.Console.WriteLine(42);")]
        public required string Code { get; set; }

        [InputLabel("Language: csharp, fsharp, vb or il")]
        [ModalTextInput("language", initValue: "csharp")]
        [RequiredInput(false)]
        public required string Language { get; set; }

        [InputLabel("Arguments")]
        [ModalTextInput("arguments", TextInputStyle.Short)]
        [RequiredInput(false)]
        public string Arguments { get; set; } = "";
    }
}
