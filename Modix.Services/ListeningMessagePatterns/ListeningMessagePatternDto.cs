using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modix.Data.Models.Core;

namespace Modix.Services.ListeningMessagePatterns
{
    public class ListeningPatternDto
    {
        private Regex _regex;

        public Regex Regex => _regex ??= BuildRegex();
        public string Pattern { get; }

        public ListeningPatternDto(string pattern)
        {
            Pattern = pattern;
        }

        private Regex BuildRegex()
        {
            return new Regex(
                pattern: Pattern,
                options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                matchTimeout: TimeSpan.FromSeconds(2));
        }
    }
}
