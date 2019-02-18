namespace Modix.Mappings
{
    public static class StringToStringIfNotUlongMapper
    {
        public static string ToStringIfNotUlong(this string value)
            => ulong.TryParse(value, out var parsed)
                ? null
                : value;
    }
}
