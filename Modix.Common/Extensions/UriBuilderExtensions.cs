using System;

namespace Modix.Common.Extensions
{
    public static class UriBuilderExtensions
    {
        /// <summary>
        /// Removes the port specification from the builder if it's the default port for the specified scheme.
        /// </summary>
        /// <returns>The UriBuilder instance for command chaining.</returns>
        public static UriBuilder RemoveDefaultPort(this UriBuilder builder)
        {
            if (builder.Uri.IsDefaultPort)
                builder.Port = -1;

            return builder;
        }
    }
}
