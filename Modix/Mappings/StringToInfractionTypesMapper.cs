using System;

using Modix.Data.Models.Moderation;

namespace Modix.Mappings
{
    public static class StringToInfractionTypesMapper
    {
        public static InfractionType[] ToInfractionTypes(this string value)
            => Enum.TryParse<InfractionType>(value, out var parsed)
                ? new[] { parsed }
                : null;
    }
}
