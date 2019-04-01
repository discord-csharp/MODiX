using System;
using System.Text;

namespace Discord
{
    /// <inheritdoc cref="LogMessage" />
    public interface ILogMessage
    {
        /// <inheritdoc cref="LogMessage.Severity" />
        LogSeverity Severity { get; }

        /// <inheritdoc cref="LogMessage.Source" />
        string Source { get; }

        /// <inheritdoc cref="LogMessage.Message" />
        string Message { get; }

        /// <inheritdoc cref="LogMessage.Exception" />
        Exception Exception { get; }

        /// <inheritdoc cref="LogMessage.ToString(StringBuilder, bool, bool, DateTimeKind, int?)" />
        string ToString(StringBuilder builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Local, int? padSource = 11);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="LogMessage"/>, through the <see cref="ILogMessage"/> interface.
    /// </summary>
    public struct LogMessageAbstraction : ILogMessage
    {
        /// <summary>
        /// Constructs a new <see cref="LogMessageAbstraction"/> around an existing <see cref="LogMessage"/>.
        /// </summary>
        /// <param name="logMessage">The existing <see cref="LogMessage"/> to be abstracted.</param>
        public LogMessageAbstraction(LogMessage logMessage)
        {
            _logMessage = logMessage;
        }

        /// <inheritdoc />
        public LogSeverity Severity
            => _logMessage.Severity;

        /// <inheritdoc />
        public string Source
            => _logMessage.Source;

        /// <inheritdoc />
        public string Message
            => _logMessage.Message;

        /// <inheritdoc />
        public Exception Exception
            => _logMessage.Exception;

        /// <inheritdoc cref="LogMessage.ToString" />
        public override string ToString()
            => _logMessage.ToString();

        /// <inheritdoc />
        public string ToString(StringBuilder builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Local, int? padSource = 11)
            => _logMessage.ToString(builder, fullException, prependTimestamp, timestampKind, padSource);

        private readonly LogMessage _logMessage;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="LogMessage"/> data.
    /// </summary>
    public static class LogMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="LogMessage"/> to an abstracted <see cref="ILogMessage"/> value.
        /// </summary>
        /// <param name="logMessage">The existing <see cref="LogMessage"/> to be abstracted.</param>
        /// <returns>An <see cref="ILogMessage"/> that abstracts <paramref name="logMessage"/>.</returns>
        public static ILogMessage Abstract(this LogMessage logMessage)
            => new LogMessageAbstraction(logMessage);
    }
}
