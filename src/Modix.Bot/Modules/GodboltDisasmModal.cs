#nullable enable
using Discord;
using Discord.Interactions;

namespace Modix.Bot.Modules
{
    public class GodboltDisasmModal : IModal
    {
        private string _language = "csharp";

        public string Title => "Godbolt";

        [InputLabel("Code")]
        [ModalTextInput("code", TextInputStyle.Paragraph, initValue: """
            using System;

            class Program
            {
                static int Square(int num) => num * num;
                static void Main() => Console.WriteLine(Square(42));
            }
            """)]
        public required string Code { get; set; }

        [InputLabel("Language: csharp, fsharp, vb or il")]
        [ModalTextInput("language", initValue: "csharp")]
        [RequiredInput(false)]
        public required string Language
        {
            get => _language;
            set => _language = value.ToLowerInvariant();
        }

        [InputLabel("Arguments")]
        [ModalTextInput("arguments", TextInputStyle.Short)]
        [RequiredInput(false)]
        public string Arguments { get; set; } = "";
    }
}
