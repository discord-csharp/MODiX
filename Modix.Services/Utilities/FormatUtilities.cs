﻿using Discord;

using Humanizer;

using Modix.Data.Models.Moderation;
using Modix.Services.AutoCodePaste;
using Modix.Services.Utilities.ColorQuantization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public static string Sanitize(string text)
            => text.Replace("@everyone", "@\x200beveryone")
                   .Replace("@here", "@\x200bhere");

        /// <summary>
        /// Identifies a dominant color from the provided image.
        /// </summary>
        /// <param name="image">The image for which the dominant color is to be retrieved.</param>
        /// <returns>A dominant color in the provided image.</returns>
        public static Color GetDominantColor(Image image)
        {
            if (image.Stream is null)
                return new Color(253, 95, 0);

            var imageBytes = new byte[image.Stream.Length].AsSpan();
            image.Stream.Seek(0, SeekOrigin.Begin);
            image.Stream.Read(imageBytes);

            var colorTree = new Octree();

            using (var img = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                for (var x = 0; x < img.Width; x++)
                {
                    for (var y = 0; y < img.Height; y++)
                    {
                        var pixel = img[x, y];

                        // Don't include transparent pixels.
                        if (pixel.A > 127)
                        {
                            var color = System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);

                            colorTree.Add(color);
                        }
                    }
                }
            }

            for (var i = 0; i < 7; i++)
            {
                colorTree.Reduce();
            }

            var mostCommonPaletteColor = colorTree.GetPalette().OrderByDescending(x => x.Weight * x.Color.GetSaturation()).FirstOrDefault().Color;

            return (Color)mostCommonPaletteColor;
        }
        
        /// <summary>
        /// Collapses plural forms into a "singular(s)"-type format.
        /// </summary>
        /// <param name="sentences">The collection of sentences for which to collapse plurals.</param>
        /// <returns>A collection of formatted sentences.</returns>
        public static IReadOnlyCollection<string> CollapsePlurals(IReadOnlyCollection<string> sentences)
        {
            var splitIntoWords = sentences.Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries));

            var commands = splitIntoWords
                .Select(x => x.First())
                .Distinct();

            var rest = splitIntoWords
                .Select(x => string.Join(" ", x.Skip(1)))
                .Distinct();

            string parenthesizedCommand;
            if (commands.Count() == 2)
            {
                var orderedCommands = commands.OrderBy(x => x.Length);
                var shortestCommand = orderedCommands.First();
                var longestCommand = orderedCommands.Last();

                var difference = longestCommand.Remove(0, shortestCommand.Length);

                parenthesizedCommand = $"{shortestCommand}({difference})";
            }
            else
            {
                parenthesizedCommand = commands.Single();
            }

            return rest.Select(x => $"{parenthesizedCommand} {x}").ToArray();
        }
    }
}
