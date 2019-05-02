using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Bot.Extensions
{
    public static class UriBuilderExtensions
    {
        public static string ToString(this UriBuilder builder, bool trimDefaultPort)
        {
            if (trimDefaultPort && builder.Uri.IsDefaultPort)
                builder.Port = -1;

            return builder.Uri.ToString();
        }
    }
}
