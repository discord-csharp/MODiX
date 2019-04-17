using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;

using Humanizer;
using Humanizer.Localisation;

using Modix.Data.Models.Moderation;
using Modix.Services.CodePaste;

namespace Modix.Services.Utilities
{
    public static class FormatUtilities
    {
        private static readonly Regex _buildContentRegex = new Regex(@"```([^\s]+|)");

        /// <summary>
        /// Prepares a piece of input code for use in HTTP operations
        /// </summary>
        /// <param name="code">The code to prepare</param>
        /// <returns>The resulting StringContent for HTTP operations</returns>
        public static StringContent BuildContent(string code)
        {
            var cleanCode = StripFormatting(code);
            return new StringContent(cleanCode, Encoding.UTF8, "text/plain");
        }

        /// <summary>
        /// Attempts to get the language of the code piece
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns>The code language if a match is found, null of none are found</returns>
        public static string GetCodeLanguage(string message)
        {
            var match = _buildContentRegex.Match(message);
            if (match.Success)
            {
                var codeLanguage = match.Groups[1].Value;
                return string.IsNullOrEmpty(codeLanguage) ? null : codeLanguage;
            }
            else
            {
                return null;
            }
        }

        public static string StripFormatting(string code)
        {
            var cleanCode = _buildContentRegex.Replace(code.Trim(), string.Empty); //strip out the ` characters and code block markers
            cleanCode = cleanCode.Replace("\t", "    "); //spaces > tabs
            cleanCode = FixIndentation(cleanCode);
            return cleanCode;
        }

        /// <summary>
        /// Attempts to fix the indentation of a piece of code by aligning the left sidie.
        /// </summary>
        /// <param name="code">The code to align</param>
        /// <returns>The newly aligned code</returns>
        public static string FixIndentation(string code)
        {
            var lines = code.Split('\n');
            var indentLine = lines.SkipWhile(d => d.FirstOrDefault() != ' ').FirstOrDefault();
            
            if (indentLine != null)
            {
                var indent = indentLine.LastIndexOf(' ') + 1;

                var pattern = $@"^[^\S\n]{{{indent}}}";

                return Regex.Replace(code, pattern, "", RegexOptions.Multiline);
            }

            return code;
        }

        public static async Task UploadToServiceIfBiggerThan(this EmbedBuilder embed, string content, string contentType, uint size, CodePasteService service)
        {
            if (content.Length > size)
            {
                try
                {
                    var resultLink = await service.UploadCodeAsync(content, contentType);
                    embed.AddField(a => a.WithName("More...").WithValue($"[View on Hastebin]({resultLink})"));
                }
                catch (WebException we)
                {
                    embed.AddField(a => a.WithName("More...").WithValue(we.Message));
                }
            }
        }

        public static string FormatInfractionCounts(IDictionary<InfractionType, int> counts)
        {
            if (counts.Values.Sum() == 0)
            {
                return "This user is clean - no active infractions!";
            }

            var formatted = 
                counts.Select(d =>
                    {
                        var formattedKey = d.Key.Humanize().ToLower();
                        return $"{d.Value} {(d.Value == 1 ? formattedKey : formattedKey.Pluralize())}";
                    })
                    .Humanize();

            return $"This user has {formatted}";
        }

        public static string SanitizeAllMentions(string text)
        {
            var everyoneSanitized = SanitizeEveryone(text);
            var userSanitized = SanitizeUserMentions(everyoneSanitized);
            var roleSanitized = SanitizeRoleMentions(userSanitized);

            return roleSanitized;
        }

        public static string SanitizeEveryone(string text)
            => text.Replace("@everyone", "@\x200beveryone")
                   .Replace("@here", "@\x200bhere");

        public static string SanitizeUserMentions(string text)
            => _userMentionRegex.Replace(text, "<@\x200b${Id}>");

        public static string SanitizeRoleMentions(string text)
            => _roleMentionRegex.Replace(text, "<@&\x200b${Id}>");

        private static readonly Regex _userMentionRegex = new Regex("<@!?(?<Id>[0-9]+)>", RegexOptions.Compiled);

        private static readonly Regex _roleMentionRegex = new Regex("<@&(?<Id>[0-9]+)>", RegexOptions.Compiled);
        
        /// <summary>
        /// Collapses plural forms into a "singular(s)"-type format.
        /// </summary>
        /// <param name="sentences">The collection of sentences for which to collapse plurals.</param>
        /// <returns>A collection of formatted sentences.</returns>
        public static IReadOnlyCollection<string> CollapsePlurals(IReadOnlyCollection<string> sentences)
        {
            var splitIntoWords = sentences.Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries));

            var withSingulars = splitIntoWords.Select(x =>
            (
                Singular: x.Select(y => y.Singularize(false)).ToArray(),
                Value: x
            ));

            var groupedBySingulars = withSingulars.GroupBy(x => x.Singular, x => x.Value, new SequenceEqualityComparer<string>());

            var withDistinctParts = new HashSet<string>[groupedBySingulars.Count()][];

            foreach (var (singular, singularIndex) in groupedBySingulars.AsIndexable())
            {
                var parts = new HashSet<string>[singular.Key.Count];

                for (var i = 0; i < parts.Length; i++)
                    parts[i] = new HashSet<string>();

                foreach (var variation in singular)
                {
                    foreach (var (part, partIndex) in variation.AsIndexable())
                    {
                        parts[partIndex].Add(part);
                    }
                }

                withDistinctParts[singularIndex] = parts;
            }

            var parenthesized = new string[withDistinctParts.Length][];

            foreach (var (alias, aliasIndex) in withDistinctParts.AsIndexable())
            {
                parenthesized[aliasIndex] = new string[alias.Length];

                foreach (var (word, wordIndex) in alias.AsIndexable())
                {
                    if (word.Count == 2)
                    {
                        var indexOfDifference = word.First()
                            .ZipOrDefault(word.Last())
                            .AsIndexable()
                            .First(x => x.Value.First != x.Value.Second)
                            .Index;

                        var longestForm = word.First().Length > word.Last().Length
                            ? word.First()
                            : word.Last();

                        parenthesized[aliasIndex][wordIndex] = $"{longestForm.Substring(0, indexOfDifference)}({longestForm.Substring(indexOfDifference)})";
                    }
                    else
                    {
                        parenthesized[aliasIndex][wordIndex] = word.Single();
                    }
                }
            }

            var formatted = parenthesized.Select(aliasParts => string.Join(" ", aliasParts)).ToArray();

            return formatted;
        }

        public static string FormatTimeAgo(DateTimeOffset now, DateTimeOffset ago)
        {
            var span = now - ago;

            var humanizedTimeAgo = span > TimeSpan.FromSeconds(60)
                ? span.Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture)
                : "a few seconds";

            return $"{humanizedTimeAgo} ago ({ago.UtcDateTime:yyyy-MM-ddTHH:mm:ssK})";
        }
    }
}
