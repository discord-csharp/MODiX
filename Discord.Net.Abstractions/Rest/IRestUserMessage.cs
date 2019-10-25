using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestUserMessage" />
    public interface IRestUserMessage : IRestMessage, IUserMessage, IDeletable
    {
        /// <inheritdoc cref="RestUserMessage.Reactions" />
        new IReadOnlyDictionary<IEmote, IReactionMetadata> Reactions { get; }
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

        /// <inheritdoc />
        public IReadOnlyDictionary<IEmote, IReactionMetadata> Reactions
            => RestUserMessage.Reactions
                .ToDictionary(x => x.Key, x => x.Value.Abstract());

        /// <inheritdoc />
        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
            => RestUserMessage.AddReactionAsync(emote, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
            => RestUserMessage.GetReactionUsersAsync(emoji, limit, options)
                .Select(x => x
                    .Select(UserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
            => RestUserMessage.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task PinAsync(RequestOptions options = null)
            => RestUserMessage.PinAsync(options);

        /// <inheritdoc />
        public Task RemoveAllReactionsAsync(RequestOptions options = null)
            => RestUserMessage.RemoveAllReactionsAsync(options);

        /// <inheritdoc />
        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
            => RestUserMessage.RemoveReactionAsync(emote, user, options);

        /// <inheritdoc />
        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
            => RestUserMessage.Resolve(userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);

        /// <inheritdoc />
        public Task UnpinAsync(RequestOptions options = null)
            => RestUserMessage.UnpinAsync(options);

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

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
