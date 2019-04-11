using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestSystemMessage" />
    public interface IRestSystemMessage : IRestMessage, ISystemMessage { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestSystemMessage"/>, through the <see cref="IRestSystemMessage"/> interface.
    /// </summary>
    internal class RestSystemMessageAbstraction : RestMessageAbstraction, IRestSystemMessage
    {
        /// <summary>
        /// Constructs a new <see cref="RestSystemMessageAbstraction"/> around an existing <see cref="Rest.RestSystemMessage"/>.
        /// </summary>
        /// <param name="restSystemMessage">The value to use for <see cref="Rest.RestSystemMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restSystemMessage"/>.</exception>
        public RestSystemMessageAbstraction(RestSystemMessage restSystemMessage)
            : base(restSystemMessage) { }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestSystemMessage"/> objects.
    /// </summary>
    internal static class RestSystemMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestSystemMessage"/> to an abstracted <see cref="IRestSystemMessage"/> value.
        /// </summary>
        /// <param name="restSystemMessage">The existing <see cref="RestSystemMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restSystemMessage"/>.</exception>
        /// <returns>An <see cref="IRestSystemMessage"/> that abstracts <paramref name="restSystemMessage"/>.</returns>
        public static IRestSystemMessage Abstract(this RestSystemMessage restSystemMessage)
            => new RestSystemMessageAbstraction(restSystemMessage);
    }
}
