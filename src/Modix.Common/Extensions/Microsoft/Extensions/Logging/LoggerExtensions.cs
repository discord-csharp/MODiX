using System;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        public static Action<ILogger> WithoutException(
                this Action<ILogger, Exception?> action)
            => logger => action.Invoke(logger, null);

        public static Action<ILogger, T> WithoutException<T>(
                this Action<ILogger, T, Exception?> action)
            => (logger, value) => action.Invoke(logger, value, null);

        public static Action<ILogger, T1, T2> WithoutException<T1, T2>(
                this Action<ILogger, T1, T2, Exception?> action)
            => (logger, value1, value2) => action.Invoke(logger, value1, value2, null);
    }
}
