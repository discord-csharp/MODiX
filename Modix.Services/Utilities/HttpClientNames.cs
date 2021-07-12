namespace Modix.Services.Utilities
{
    /// <summary>
    /// Provides name values for each named HttpClient that is configured in the application.
    /// </summary>
    public static class HttpClientNames
    {
        public static string RetryOnTransientErrorPolicy { get; } = nameof(RetryOnTransientErrorPolicy);

        public static string AutomaticGZipDecompression { get; } = nameof(AutomaticGZipDecompression);

        public static string TimeoutFiveSeconds { get; } = nameof(TimeoutFiveSeconds);

        public static string Timeout300ms { get; } = nameof(Timeout300ms);
    }
}
