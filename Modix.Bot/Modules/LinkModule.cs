#nullable enable

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Services.Utilities;

using LZStringCSharp;

namespace Modix.Bot.Modules
{
    [Name("Link")]
    [Summary("Commands for working with links.")]
    public class LinkModule : ModuleBase
    {
        [Command("link")]
        [Alias("url", "uri", "shorten", "linkto")]
        [Summary("Shortens the provided link.")]
        public async Task LinkAsync(
            [Summary("The link to shorten.")]
                Uri uri)
        {
            var host = uri.Host;

            if (!_allowedHosts.Contains(host))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"Links to {host} cannot be shortened.")
                    .Build());

                return;
            }

            var urlMarkdown = Format.Url($"{host} (click here)", uri.ToString());

            var description = host.Equals("sharplab.io") && TryPrepareSharplabPreview(uri.OriginalString, urlMarkdown.Length + 1, out var preview)
                ? $"{urlMarkdown}\n{preview}"
                : urlMarkdown;

            await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(description)
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.LightGrey)
                .Build());

            await Context.Message.DeleteAsync();
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
                string base64Text = url.Substring(url.IndexOf("#v2:") + "#v2:".Length);
                string plainText = LZString.DecompressFromBase64(base64Text);

                // Extract the option and get the target language
                var textParts = Regex.Match(plainText, @"([^|]*)\|([\s\S]*)$");
                var languageOption = Regex.Match(textParts.Groups[1].Value, @"l:(\w+)");
                string language = languageOption.Success ? languageOption.Groups[1].Value : "cs";
                string sourceCode = textParts.Groups[2].Value;

                // Replace the compression tokens
                if (language is "cs")
                {
                    sourceCode = Regex.Replace(sourceCode, @"@(\d+|@)", match =>
                    {
                        if (match.Value is "@@") return "@";

                        return _sharplabCSTokens[int.Parse(match.Groups[1].Value)];
                    });

                    // Strip using directives
                    sourceCode = Regex.Replace(sourceCode, @"using \w+(?:\.\w+)*;", string.Empty);
                }
                else sourceCode = sourceCode.Replace("@@", "@");

                // Trim, for good measure
                sourceCode = sourceCode.Trim();

                // Clip to avoid Discord errors
                int maximumLength = EmbedBuilder.MaxDescriptionLength - (markdownLength + language.Length + "```\n\n```".Length);

                if (sourceCode.Length > maximumLength)
                {
                    // Clip at the maximum length
                    sourceCode = sourceCode.Substring(0, maximumLength);

                    int lastCarriageIndex = sourceCode.LastIndexOf('\n');

                    // Remove the last line to avoid having code cut mid-statements
                    if (lastCarriageIndex > 0)
                    {
                        sourceCode = sourceCode.Substring(0, lastCarriageIndex).Trim();
                    }
                }

                preview = Format.Code(sourceCode, language);

                return true;
            }
            catch
            {
                preview = null;

                return false;
            }
        }

        private static readonly ImmutableArray<string> _allowedHosts
            = ImmutableArray.Create
            (
                "sharplab.io",
                "docs.microsoft.com",
                "www.docs.microsoft.com"
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
    }
}
