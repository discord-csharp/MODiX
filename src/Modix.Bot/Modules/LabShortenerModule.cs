#nullable enable

using System;
using System.Buffers.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using LZStringCSharp;
using Modix.Bot.Responders.AutoRemoveMessages;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using ProtoBuf;

namespace Modix.Bot.Modules
{
    [ModuleHelp("LabShortener", "Commands for working with sharplab.io/lab.razor.fyi.")]
    public partial class LabShortenerModule : InteractionModuleBase
    {
        private readonly AutoRemoveMessageService _autoRemoveMessageService;

        public LabShortenerModule(AutoRemoveMessageService autoRemoveMessageService)
        {
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [SlashCommand("shorten", "Shortens the provided link.")]
        public async Task LinkAsync(
            [Summary(description: "The link to shorten.")]
            Uri uri)
        {
            var host = uri.Host;

            if (!_allowedHosts.Contains(host))
            {
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"Links to {host} cannot be shortened.")
                    .Build());

                return;
            }

            var urlMarkdown = Format.Url($"{host} (click here)", uri.ToString());

            string? preview = null;
            var previewSuccess = host switch
            {
                "sharplab.io" => TryPrepareSharplabPreview(uri.Fragment, urlMarkdown.Length + 1, out preview),
                "lab.razor.fyi" => TryPrepareRazorLabPreview(uri.Fragment, urlMarkdown.Length + 1, out preview),
                _ => throw new UnreachableException("already checked for other hosts"),
            };

            var description = previewSuccess
                ? $"{urlMarkdown}\n{preview}"
                : urlMarkdown;

            if (description.Length > EmbedBuilder.MaxDescriptionLength)
            {
                await FollowupAsync("Error: The provided link is too long to be converted to an embed.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription(description)
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.LightGrey);

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.User, embed, async e => await FollowupAsync(embed: e.Build()));
        }

        private static bool TryPrepareSharplabPreview(string fragment, int markdownLength, [NotNullWhen(true)] out string? preview)
        {
            if (!fragment.StartsWith("#v2:"))
            {
                preview = null;
                return false;
            }

            try
            {
                // Decode the compressed code from the URL payload
                var base64Text = fragment[4..];
                var plainText = LZString.DecompressFromBase64(base64Text);

                // Extract the option and get the target language
                var textParts = Regex.Match(plainText, @"([^|]*)\|([\s\S]*)$");
                var languageOption = Regex.Match(textParts.Groups[1].Value, @"l:(\w+)");
                var language = languageOption.Success ? languageOption.Groups[1].Value : "cs";
                var sourceCode = textParts.Groups[2].Value;

                // Replace the compression tokens
                if (language is "cs")
                {
                    sourceCode = ReplaceTokens(sourceCode, _sharplabCSTokens);

                    // Strip using directives
                    sourceCode = RemoveUsings(sourceCode);
                }
                else if (language is "il")
                    sourceCode = ReplaceTokens(sourceCode, _sharplabILTokens);
                else
                    sourceCode = sourceCode.Replace("@@", "@");

                var maxPreviewLength = EmbedBuilder.MaxDescriptionLength - (markdownLength + language.Length + "```\n\n```".Length);

                preview = FormatUtilities.FormatCodeForEmbed(language, sourceCode, maxPreviewLength);

                return !string.IsNullOrWhiteSpace(preview);
            }
            catch
            {
                preview = null;
                return false;
            }
        }

        private static bool TryPrepareRazorLabPreview(string fragment, int markdownLength, [NotNullWhen(true)] out string? preview)
        {
            if (string.IsNullOrWhiteSpace(fragment))
            {
                preview = null;
                return false;
            }

            try
            {
                var bytes = Base64Url.DecodeFromChars(fragment.AsSpan(1));
                using var deflateStream = new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress);

                var savedState = Serializer.Deserialize<RazorLabSavedState>(deflateStream);
                var selectedFile = savedState.Inputs[savedState.SelectedInputIndex];

                if (selectedFile.FileExtension != ".cs")
                {
                    preview = null;
                    return false;
                }

                var source = RemoveUsings(selectedFile.Text);

                var maxPreviewLength = EmbedBuilder.MaxDescriptionLength - (markdownLength + "```cs\n\n```".Length);

                preview = FormatUtilities.FormatCodeForEmbed("cs", source, maxPreviewLength);

                return !string.IsNullOrWhiteSpace(preview);
            }
            catch
            {
                preview = null;
                return false;
            }
        }

        [GeneratedRegex(@"using \w+(?:\.\w+)*;")]
        private static partial Regex UsingsRegex();
        private static string RemoveUsings(string sourceCode) => UsingsRegex().Replace(sourceCode, "");

        private static string ReplaceTokens(string sourceCode, ImmutableArray<string> tokens)
        {
            return Regex.Replace(sourceCode, @"@(\d+|@)", match =>
            {
                if (match.Value is "@@")
                    return "@";

                return tokens[int.Parse(match.Groups[1].Value)];
            });
        }

        private static readonly ImmutableArray<string> _allowedHosts
            = [
                "sharplab.io",
                "lab.razor.fyi"
            ];

        private static readonly ImmutableArray<string> _sharplabCSTokens
            = [
                "using",
                "System",
                "class",
                "public",
                "void",
                "Func",
                "Task",
                "return",
                "async",
                "await",
                "string",
                "yield",
                "Action",
                "IEnumerable",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "static",
                "Program",
                "Main",
                "Console.WriteLine",
                "", // <help.run.csharp>
                "using System;",
                "public static void Main()",
                "public static class Program",
                "Inspect.Allocations(() =>",
                "Inspect.MemoryGraph("
            ];

        private static readonly ImmutableArray<string> _sharplabILTokens
            = [
                "Main ()",
                "Program",
                "ConsoleApp",
                "cil managed",
                ".entrypoint",
                ".maxstack",
                ".assembly",
                ".class public auto ansi abstract sealed beforefieldinit",
                "extends System.Object",
                ".method public hidebysig",
                "call void [System.Console]System.Console::WriteLine("
            ];
    }

    // the below definitions were taken from https://github.com/jjonescz/DotNetLab at commit dedcefec241a1d32fe8a6683ccaa39ff40dc1730
    // as such they are licensed under the MIT license in that repo, https://github.com/jjonescz/DotNetLab/blob/dedcefec241a1d32fe8a6683ccaa39ff40dc1730/LICENSE
    [ProtoContract]
    sealed file record RazorLabInputCode
    {
        [ProtoMember(1)]
        public required string FileName { get; init; }
        [ProtoMember(2)]
        public required string Text { get; init; }

        public string FileExtension => Path.GetExtension(FileName);
    }

    [ProtoContract]
    sealed file record RazorLabSavedState
    {
        [ProtoMember(1)]
        public ImmutableArray<RazorLabInputCode> Inputs { get; init; }

        [ProtoMember(8)]
        public int SelectedInputIndex { get; init; }
    }
}
