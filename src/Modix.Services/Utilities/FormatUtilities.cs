using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

namespace Modix.Services.Utilities
{
    public static class FormatUtilities
    {
        private static readonly Regex _buildContentRegex = new(@"```([^\s]*)", RegexOptions.Compiled);

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

        public static string StripFormatting(string code) =>
            //strip out the ` characters and code block markers
            _buildContentRegex.Replace(code.Trim(), string.Empty);

        public static async Task UploadToServiceIfBiggerThan(this EmbedBuilder embed, string content, uint size,
            PasteService service)
        {
            if (content.Length > size)
            {
                var resultLink = await service.UploadPaste(content);

                if (!string.IsNullOrWhiteSpace(resultLink))
                {
                    embed.AddField(a => a.WithName("More...").WithValue($"[View on paste.mod.gg]({resultLink})"));
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

            var groupedBySingulars =
                withSingulars.GroupBy(x => x.Singular, x => x.Value, new SequenceEqualityComparer<string>());

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

                        parenthesized[aliasIndex][wordIndex] =
                            $"{longestForm[..indexOfDifference]}({longestForm[indexOfDifference..]})";
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

        public static bool ContainsSpoiler(string text)
            => _containsSpoilerRegex.IsMatch(text);

        private static readonly Regex _containsSpoilerRegex
            = new(@"\|\|.+\|\|", RegexOptions.Compiled);

#nullable enable
        public static string FormatCodeForEmbed(string language, string sourceCode, int maxLength)
        {
            if (maxLength <= 0)
                return string.Empty;

            // Trim, for good measure
            sourceCode = sourceCode.Trim();
            var processedLines = new List<string>();
            var braceOnlyLinesEliminated = 0;
            var currentLength = 0;

            var lines = sourceCode
                .Replace("\r", string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.TrimEnd())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToImmutableArray();

            // Embeds always end up being too long and then messy to display in chat. They also disrupt conversations
            // a lot as they just end up being a huge wall of text out of nowhere. To account for this, we will:
            //   - Trim all lines at the right side
            //   - Remove blank lines
            //   - Change all single-line opening brackets to be on the previous line instead
            //   - Clip every line to the maximum length in an embed
            //   - Clip the total number of lines to 10
            //   - If there's more, add a comment indicating the number of remaining lines
            foreach (var line in lines)
            {
                const int MaxLineLength = 61;

                if (line.Length < MaxLineLength - 1)
                {
                    if (line.EndsWith('{') &&
                        string.IsNullOrWhiteSpace(line[..^1]) &&
                        processedLines.Count > 0)
                    {
                        if (!TryReplaceLine(^1, processedLines[^1] + " {"))
                        {
                            AddRemainingLineComment();
                            break;
                        }

                        braceOnlyLinesEliminated++;
                    }
                    else if (!TryAddLine(line))
                    {
                        AddRemainingLineComment();
                        break;
                    }
                }
                else
                {
                    if (!TryAddLine(line[..(MaxLineLength - 3)] + "..."))
                    {
                        AddRemainingLineComment();
                        break;
                    }
                }

                if (processedLines.Count == 10)
                {
                    var remainingCount = GetRemainingLineCount();

                    // Might as well just add the last line to the embed,
                    // since we'd just be adding a "1 more line" line otherwise.
                    // We'd be adding a line either way, so we should go with the
                    // more useful option.
                    if (remainingCount == 1)
                        continue;

                    if (remainingCount <= 0)
                        break;

                    AddRemainingLineComment();
                    break;
                }
            }

            var code = string.Join('\n', processedLines);
            return Format.Code(code, language);

            bool TryAddLine(string line)
            {
                var remainingCount = GetRemainingLineCount();
                var possibleRemainingLineCommentLength =
                    remainingCount > 1 // 1, because the current line is included in the count
                        ? GetRemainingLineCountComment(remainingCount).Length
                        : 0;

                if (line.Length + currentLength + possibleRemainingLineCommentLength + 1 >
                    maxLength) // +1 because of the newline that will be added later
                    return false;

                processedLines.Add(line);
                currentLength += line.Length + 1; // +1 because of the newline that will be added later
                return true;
            }

            bool TryReplaceLine(Index index, string line)
            {
                var remainingCount = GetRemainingLineCount();
                var possibleRemainingLineCommentLength = remainingCount > 0
                    ? GetRemainingLineCountComment(remainingCount).Length
                    : 0;

                var lengthDifference = line.Length - processedLines[index].Length;

                if (lengthDifference + currentLength + possibleRemainingLineCommentLength > maxLength)
                    return false;

                processedLines[index] = line;
                currentLength += lengthDifference;
                return true;
            }

            int GetRemainingLineCount()
            {
                return lines.Length - processedLines.Count - braceOnlyLinesEliminated;
            }

            string GetRemainingLineCountComment(int remainingCount)
            {
                var commentStart = language switch
                {
                    "vb" => "'",
                    _ => "//",
                };

                return $"{commentStart} {remainingCount} more lines. Follow the link to view.";
            }

            void AddRemainingLineComment()
            {
                processedLines.Add(GetRemainingLineCountComment(GetRemainingLineCount()));
            }
        }
    }
}
