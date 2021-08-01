using System;
using Remora.Discord.Core;

namespace Remora.Discord.API.Abstractions.Objects
{
    public static class RemoraExtensions
    {
        public static T? GetOptionalValueOrDefault<T>(this Optional<T>? optional, T? defaultValue = default) where T: class
        {
            if (!optional.HasValue || !optional.Value.HasValue)
            {
                return defaultValue;
            }
            return optional.Value.Value;
        }

        public static T GetOptionalValueOrDefault<T>(this Optional<T?>? optional, T defaultValue = default) where T : struct
        {
            if (!optional.HasValue || !optional.Value.HasValue || !optional.Value.Value.HasValue)
            {
                return defaultValue;
            }

            return optional.Value.Value.Value;
        }
    }
}

