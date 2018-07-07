using System;
using System.Text.RegularExpressions;

namespace Modix.Common
{
    public static class StringExtensions
    {
        public static Regex CheckMatch(this string pattern)
            => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
    }
}
