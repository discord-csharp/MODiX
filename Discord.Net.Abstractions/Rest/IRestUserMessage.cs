using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestUserMessage" />
    public interface IRestUserMessage : IRestMessage, IUserMessage, IDeletable
    {
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestUserMessage"/>, through the <see cref="IRestUserMessage"/> interface.
    /// </summary>
    internal class RestUserMessageAbstraction : RestMessageAbstraction, IRestUserMessage
    {
        /// <summary>
        /// Constructs a new <see cref="RestUserMessageAbstraction"/> around an existing <see cref="Rest.RestUserMessage"/>.
        /// </summary>
        /// <param name="restUserMessage">The value to use for <see cref="Rest.RestUserMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUserMessage"/>.</exception>
        public RestUserMessageAbstraction(RestUserMessage restUserMessage)
            : base(restUserMessage) { }

        public Task CrosspostAsync(RequestOptions options = null)
            => RestUserMessage.CrosspostAsync(options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
            => RestUserMessage.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task PinAsync(RequestOptions options = null)
            => RestUserMessage.PinAsync(options);

        /// <inheritdoc />
        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
            => RestUserMessage.Resolve(userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);

        /// <inheritdoc />
        public Task UnpinAsync(RequestOptions options = null)
            => RestUserMessage.UnpinAsync(options);

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions options = null)
            => RestUserMessage.ModifySuppressionAsync(suppressEmbeds, options);

        /// <summary>
        /// The existing <see cref="Rest.RestUserMessage"/> being abstracted.
        /// </summary>
        protected RestUserMessage RestUserMessage
            => RestMessage as RestUserMessage;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestUserMessage"/> objects.
    /// </summary>
    internal static class RestUserMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestUserMessage"/> to an abstracted <see cref="IRestUserMessage"/> value.
        /// </summary>
        /// <param name="restUserMessage">The existing <see cref="RestUserMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUserMessage"/>.</exception>
        /// <returns>An <see cref="IRestUserMessage"/> that abstracts <paramref name="restUserMessage"/>.</returns>
        public static IRestUserMessage Abstract(this RestUserMessage restUserMessage)
            => new RestUserMessageAbstraction(restUserMessage);
    }
}
