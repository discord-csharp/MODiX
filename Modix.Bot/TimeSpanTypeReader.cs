﻿using System;
using System.Globalization;

namespace Modix
{
    // TODO: Remove when we port to 2.0
    internal static class TimeSpanTypeReader
    {
        private static readonly string[] _formats = new[]
        {
            "%d'd'%h'h'%m'm'%s's'", //4d3h2m1s
            "%d'd'%h'h'%m'm'",      //4d3h2m
            "%d'd'%h'h'%s's'",      //4d3h  1s
            "%d'd'%h'h'",           //4d3h
            "%d'd'%m'm'%s's'",      //4d  2m1s
            "%d'd'%m'm'",           //4d  2m
            "%d'd'%s's'",           //4d    1s
            "%d'd'",                //4d
            "%h'h'%m'm'%s's'",      //  3h2m1s
            "%h'h'%m'm'",           //  3h2m
            "%h'h'%s's'",           //  3h  1s
            "%h'h'",                //  3h
            "%m'm'%s's'",           //    2m1s
            "%m'm'",                //    2m
            "%s's'",                //      1s
        };

        public static TimeSpan? Read(string input)
        {
            if (TimeSpan.TryParseExact(input.ToLowerInvariant(), _formats, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return timeSpan;
            }

            return null;
        }
    }
}
