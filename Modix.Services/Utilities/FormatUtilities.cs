using Discord;
using Humanizer;
using Modix.Data.Models.Moderation;
using Modix.Services.AutoCodePaste;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public static unsafe Discord.Color GetDominantColor(Discord.Image image)
        {
            if (image.Stream is null)
                return new Discord.Color(253, 95, 0);

            var colorCounts = new Dictionary<System.Drawing.Color, int>();
            BitmapData data = default;

            using (var img = System.Drawing.Image.FromStream(image.Stream))
            using (var bmp = new Bitmap(img))
            {
                try
                {
                    data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    var sourceRowPtr = (byte*)data.Scan0.ToPointer();

                    for (var row = 0; row < bmp.Height; row++, sourceRowPtr += data.Stride)
                    {
                        var sourcePixelPtr = (int*)sourceRowPtr;

                        for (var column = 0; column < bmp.Width; column++)
                        {
                            var sourcePixel = *(sourcePixelPtr + column);

                            // 4 bits are stripped off the end of each to account for similar colors
                            var a = (byte)(((sourcePixel & 0xFF000000) >> 28) << 4);
                            var r = (byte)(((sourcePixel & 0x00FF0000) >> 20) << 4);
                            var g = (byte)(((sourcePixel & 0x0000FF00) >> 12) << 4);
                            var b = (byte)(((sourcePixel & 0x000000FF) >> 4) << 4);

                            // Don't include transparent pixels
                            if (a > 127)
                            {
                                var color = System.Drawing.Color.FromArgb(a, r, g, b);

                                if (colorCounts.TryGetValue(color, out var count))
                                {
                                    colorCounts[color] = count + 1;
                                }
                                else
                                {
                                    colorCounts[color] = 1;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }

            var mostCommonColor = colorCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key;

            return (Discord.Color)mostCommonColor;
        }
    }
}
