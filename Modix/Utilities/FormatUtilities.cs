using Discord;
using Modix.Services.AutoCodePaste;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modix.Utilities
{
    public static class FormatUtilities
    {
        private static Regex _buildContentRegex = new Regex(@"```([^\s]+|)");

        /// <summary>
        /// Prepares a piece of input code for use in HTTP operations
        /// </summary>
        /// <param name="code">The code to prepare</param>
        /// <returns>The resulting StringContent for HTTP operations</returns>
        public static StringContent BuildContent(string code)
        {
            var cleanCode = StipFormatting(code);
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
                string codeLanguage = match.Groups[1].Value;
                return string.IsNullOrEmpty(codeLanguage) ? null : codeLanguage;
            }
            else
            {
                return null;
            }
        }

        public static string StipFormatting(string code)
        {
            string cleanCode = _buildContentRegex.Replace(code.Trim(), string.Empty); //strip out the ` characters and code block markers
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
            string indentLine = lines.SkipWhile(d => d.FirstOrDefault() != ' ').FirstOrDefault();
            
            if (indentLine != null)
            {
                int indent = indentLine.LastIndexOf(' ') + 1;

                string pattern = $@"^[^\S\n]{{{indent}}}";

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
                    string resultLink = await service.UploadCode(content, contentType);
                    embed.AddField(a => a.WithName("More...").WithValue($"[View on Hastebin]({resultLink})"));
                }
                catch (WebException we)
                {
                    embed.AddField(a => a.WithName("More...").WithValue(we.Message));
                }
            }
        }
    }
}
