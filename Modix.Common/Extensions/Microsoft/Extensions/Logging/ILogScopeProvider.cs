using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Describes an object that is capable of creating a logging scope upon an <see cref="ILogger"/>,
    /// containing state information from itself.
    /// </summary>
    public interface ILogScopeProvider
    {
        /// <summary>
        /// Creates a new logging scope upon a given <see cref="ILogger"/>, containing state information from itself.
        /// </summary>
        /// <param name="logger">The logger upon which a scope is to be created.</param>
        /// <returns>An object to be disposed when the logging scope should be destroyed.</returns>
        IDisposable BeginLogScope(ILogger logger);
    }
}
