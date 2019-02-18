namespace Modix.Mappings
{
    public static class StringToUlongMapper
    {
        public static ulong? ToUlong(this string value)
            => ulong.TryParse(value, out var parsed)
                ? parsed
                : (ulong?)null;
    }
}
