using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Modix.Bot.Preconditions
{
    internal static class NumericTypes
    {
        public static Dictionary<Type, Func<object, long, int>> Comparators { get; } = new Dictionary<Type, Func<object, long, int>>()
        {
            [typeof(sbyte)] = (object a, long b) => ((long)(sbyte)a).CompareTo(b),
            [typeof(byte)] = (object a, long b) => ((long)(byte)a).CompareTo(b),
            [typeof(short)] = (object a, long b) => ((long)(short)a).CompareTo(b),
            [typeof(ushort)] = (object a, long b) => ((long)(ushort)a).CompareTo(b),
            [typeof(int)] = (object a, long b) => ((long)(int)a).CompareTo(b),
            [typeof(uint)] = (object a, long b) => ((long)(uint)a).CompareTo(b),
            [typeof(long)] = (object a, long b) => ((long)a).CompareTo(b),
        };

        public static bool IsSupportedNumericType<T>() => IsSupportedNumericType(typeof(T));

        public static bool IsSupportedNumericType(Type numericType)
            => Comparators.ContainsKey(numericType);
    }
}
