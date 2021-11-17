#nullable enable

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Services.Utilities;

using LZStringCSharp;
using System.Text;
using System.Collections.Generic;
using Modix.Services.AutoRemoveMessage;

namespace Modix.Bot.Modules
{
    [Name("Link")]
    [Summary("Commands for working with links.")]
    public class LinkModule : ModuleBase
    {
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public LinkModule(IAutoRemoveMessageService autoRemoveMessageService)
        {
            _autoRemoveMessageService = autoRemoveMessageService;
        }

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

            var embed = new EmbedBuilder()
                .WithDescription(description)
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.LightGrey);
            ;

            var message = await ReplyAsync(embed: embed.Build());

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

                var processedLines = new List<string>();
                var lines = sourceCode.Remove('\r').Split('\n', StringSplitOptions.RemoveEmptyEntries);

                // Embeds always end up being too long and then messy to display in chat. They also disrupt conversations
                // a lot as they just end up being a huge wall of text out of nowhere. To account for this, we will:
                //   - Trim all lines at the right side
                //   - Remove blank lines
                //   - Change all single-line opening brackets to be on the previous line instead
                //   - Clip every line to the maximum length in an embed
                //   - Clip the total number of lines to 10
                //   - If there's more, add a comment indicating the number of remaining lines
                for (int i = 0; i < lines.Length; i++)
                {
                    const int MaxLineLength = 57;
                    string line = lines[i].TrimEnd();

                    if (line.Length < MaxLineLength - 1)
                    {
                        if (line.EndsWith('{') &&
                            string.IsNullOrWhiteSpace(line.Substring(0, line.Length - 1)) &&
                            processedLines.Count > 0)
                        {
                            processedLines[processedLines.Count - 1] = processedLines[processedLines.Count - 1] + " {";
                        }
                        else
                        {
                            processedLines.Add(line);
                        }
                    }
                    else
                    {
                        processedLines.Add(line.Substring(0, line.Length - 3) + "...");
                    }

                    if (processedLines.Count == 10)
                    {
                        if (language is "cs")
                        {
                            processedLines.Add($"// [{lines.Length - i}] more line(s), click to expand");
                        }

                        break;
                    }
                }

                var builder = new StringBuilder();

                // Now move the processed line to a single text block
                foreach (var line in processedLines)
                {
                    builder.AppendLine(line);
                }

                preview = Format.Code(builder.ToString(), language);

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
