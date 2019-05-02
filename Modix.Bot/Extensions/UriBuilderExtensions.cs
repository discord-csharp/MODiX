using System;

namespace Modix.Bot.Extensions
{
    public static class UriBuilderExtensions
    {
        /// <summary>
        /// Returns the display string for the specified <see cref="UriBuilder"/> instance.
        /// </summary>
        /// <param name="trimDefaultPort">True to trim the default port; otherwise false.</param>
        /// <remarks>If <param name="trimDefaultPort" /> is false, the result is the same as running <see cref="UriBuilder.ToString"/>.</remarks>
        /// <returns>The UriBuilder instance for command chaining.</returns>
        public static UriBuilder RemoveDefaultPort(this UriBuilder builder)
        {
            if (builder.Uri.IsDefaultPort)
                builder.Port = -1;

            return builder;
        }
    }
}
