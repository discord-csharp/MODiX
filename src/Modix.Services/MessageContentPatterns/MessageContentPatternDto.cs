using System;
using System.Text.RegularExpressions;
using Modix.Data.Models.Core;

namespace Modix.Services.MessageContentPatterns
{
    public class MessageContentPatternDto
    {
        public MessageContentPatternDto(string pattern, MessageContentPatternType patternType)
        {
            Pattern = pattern;
            Type = patternType;
        }

        private Regex _regex;

        public Regex Regex => _regex ??= BuildRegex();
        public string Pattern { get; }
        public MessageContentPatternType Type { get; }

        private Regex BuildRegex()
        {
            return new Regex(
                pattern: Pattern,
                options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                matchTimeout: TimeSpan.FromSeconds(2));
        }
    }
}
