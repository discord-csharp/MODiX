using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Modix.Data.Utilities
{
    public static class Extensions
    {
        public static string Truncate(this string value, int maxLength, int maxLines, string suffix = "…")
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.Length <= maxLength)
            {
                return value;
            }

            var lines = value.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                
            return lines.Length > maxLines ? string.Join("\n", lines.Take(maxLines)) : $"{value.Substring(0, maxLength).Trim()}{suffix}";
        }

        public static bool OrdinalContains(this string value, string search)
        {
            if (value is null || search is null)
                return false;

            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool OrdinalEquals(this string value, string search)
            => string.Equals(value, search, StringComparison.OrdinalIgnoreCase);
    }
}
