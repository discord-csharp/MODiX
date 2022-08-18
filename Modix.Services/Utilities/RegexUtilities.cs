using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modix.Services.Utilities
{
    public static class RegexUtilities
    {
        /// <summary>
        /// Test if a <paramref name="candidate"/> <see langword="string"/> is a valid <see cref="Regex"/>.
        /// </summary>
        /// <param name="candidate">The string to test.</param>
        /// <returns><see langword="true"/> if it's a valid <see cref="Regex"/>, <see langword="false"/> otherwise.</returns>
        public static bool IsValidRegex(string candidate)
        {
            try
            {
                _ = new Regex(candidate);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
