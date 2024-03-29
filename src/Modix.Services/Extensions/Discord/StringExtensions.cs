#nullable enable

using System;
using System.Collections.Generic;

namespace Discord
{
    public static class StringExtensions
    {
        public static IEnumerable<EmbedFieldBuilder> EnumerateLongTextAsFieldBuilders(
            this string text,
            string fieldName)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Cannot be empty", nameof(text));
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Cannot be empty", nameof(fieldName));

            for(var i = 0; i < text.Length; i += EmbedFieldBuilder.MaxFieldValueLength)
                yield return new EmbedFieldBuilder()
                    .WithName((i == 0) ? fieldName : "(continued)")
                    .WithValue(text[i..Math.Min(text.Length, (i + EmbedFieldBuilder.MaxFieldValueLength))]);
        }
    }
}
