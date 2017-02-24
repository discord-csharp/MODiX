using System;

namespace Modix.Data.Utilities
{
    public static class Extensions
    {
        public static long ToLong(this ulong number)
        {
            if (!long.TryParse((number.ToString()), out long convertedNumber)) 
                throw new AggregateException("Could not convert ulong to long");

            return convertedNumber;
        }

        public static ulong ToUlong(this long number)
        {
            if (!ulong.TryParse((number.ToString()), out ulong convertedNumber))
                throw new AggregateException("Could not convert long to ulong");

            return convertedNumber;
        }
    }
}
