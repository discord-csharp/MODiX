namespace Modix.Mappings
{
    public static class StringToLongMapper
    {
        public static long? ToLong(this string value)
            => long.TryParse(value, out var parsed)
                ? parsed
                : (long?)null;
    }
}
