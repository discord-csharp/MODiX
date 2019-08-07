using System;

namespace Modix.Bot.Preconditions
{
    internal static class Preconditions
    {
        public static void AllowedNumericType(Type type, string typeName, string paramName)
        {
            if (!NumericTypes.IsSupportedNumericType(type))
                throw new ArgumentException($"{typeName} only supports one of {string.Join(", ", NumericTypes.Comparators.Keys)}", paramName);
        }
    }
}
