#nullable enable

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using LZStringCSharp;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Bot.Modules
{
    [ModuleHelp("SharpLab", "Commands for working with SharpLab.")]
    public class SharpLabModule : InteractionModuleBase
    {
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public SharpLabModule(IAutoRemoveMessageService autoRemoveMessageService)
        {
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [SlashCommand("sharplab", "Shortens the provided link.")]
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

            var description = host.Equals("sharplab.io") && TryPrepareSharplabPreview(uri.OriginalString, urlMarkdown.Length + 1, out var preview)
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

        private static bool TryPrepareSharplabPreview(string url, int markdownLength, out string? preview)
        {
            if (!url.Contains("#v2:"))
            {
                preview = null;
                return false;
            }

            try
            {
                // Decode the compressed code from the URL payload
                var base64Text = url.Substring(url.IndexOf("#v2:") + "#v2:".Length);
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
                    sourceCode = Regex.Replace(sourceCode, @"using \w+(?:\.\w+)*;", string.Empty);
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
            = ImmutableArray.Create
            (
                "sharplab.io"
            );

        private static readonly ImmutableArray<string> _sharplabCSTokens
            = ImmutableArray.Create(new[]
            {
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
            });

        private static readonly ImmutableArray<string> _sharplabILTokens
            = ImmutableArray.Create(new[]
            {
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
            });
    }
}
