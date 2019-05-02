using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Bot.Extensions
{
    public static class UriBuilderExtensions
    {
        /// <summary>
        /// Returns the display string for the specified <see cref="UriBuilder"/> instance.
        /// </summary>
        /// <param name="trimDefaultPort">True to trim the default port; otherwise false.</param>
        /// <remarks>If <param name="trimDefaultPort" /> is false, the result is the same as running <see cref="UriBuilder.ToString"/>.</remarks>
        /// <returns></returns>
        public static string ToString(this UriBuilder builder, bool trimDefaultPort)
        {
            if (trimDefaultPort && builder.Uri.IsDefaultPort)
                builder.Port = -1;

            return builder.Uri.ToString();
        }
    }
}
