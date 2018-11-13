namespace NSubstitute
{
    public static class SubstituteAssertions
    {
        public static T ShouldHaveReceived<T>(this T substitute) where T : class
            => substitute.Received();

        public static T ShouldHaveReceived<T>(this T substitute, int requiredNumberOfCalls) where T : class
            => substitute.Received(requiredNumberOfCalls);
    }
}
