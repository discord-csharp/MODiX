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

            if (host.Equals("sharplab.io") &&
                TryPrepareSharplabPreview(uri.OriginalString, out string preview))
            {
                string markdownUrl = Format.Url($"{host} (click here)", uri.ToString());
                string markdownText = $"{markdownUrl}\r{preview}";

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithDescription(markdownText)
                    .WithUserAsAuthor(Context.User)
                    .WithColor(Color.LightGrey)
                    .Build());
            }
            else
            {
                await ReplyAsync(embed: new EmbedBuilder()
                .WithDescription(Format.Url($"{host} (click here)", uri.ToString()))
                .WithUserAsAuthor(Context.User)
                .WithColor(Color.LightGrey)
                .Build());
            }

            await Context.Message.DeleteAsync();
        }

        private static bool TryPrepareSharplabPreview(string url, out string preview)
        {
            if (!url.Contains("#v2:"))
            {
                preview = null;

                return false;
            }

            try
            {
                // Decode the compressed code from the URL payload
                string base64Text = Url.Substring(Url.IndexOf("#v2:") + "#v2:".Length);
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
                }
                else sourceCode = sourceCode.Replace("@@", "@");

                preview = $"```{language}\r{sourceCode}\r```";

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
                "<help.run.csharp>",
                "using System;",
                "public static void Main()",
                "public static class Program",
                "Inspect.Allocations(() =>",
                "Inspect.MemoryGraph("
            });
    }
}
