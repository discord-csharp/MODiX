using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketUserMessage" />
    public interface ISocketUserMessage : ISocketMessage, IUserMessage
    {
        /// <inheritdoc cref="SocketUserMessage.Reactions" />
        new IReadOnlyDictionary<IEmote, IReactionMetadata> Reactions { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketUserMessage"/>, through the <see cref="ISocketUserMessage"/> interface.
    /// </summary>
    public class SocketUserMessageAbstraction : SocketMessageAbstraction, ISocketUserMessage
    {
        /// <summary>
        /// Constructs a new <see cref="SocketUserMessageAbstraction"/> around an existing <see cref="WebSocket.SocketUserMessage"/>.
        /// </summary>
        /// <param name="socketUserMessage">The value to use for <see cref="WebSocket.SocketUserMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUserMessage"/>.</exception>
        public SocketUserMessageAbstraction(SocketUserMessage socketUserMessage)
            : base(socketUserMessage) { }

        /// <inheritdoc />
        public IReadOnlyDictionary<IEmote, IReactionMetadata> Reactions
            => SocketUserMessage.Reactions
                .ToDictionary(x => x.Key, x => x.Value.Abstract());

        /// <inheritdoc />
        IReadOnlyDictionary<IEmote, ReactionMetadata> IUserMessage.Reactions
            => (SocketUserMessage as IUserMessage).Reactions;

        /// <inheritdoc />
        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
            => SocketUserMessage.AddReactionAsync(emote, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
            => SocketUserMessage.GetReactionUsersAsync(emoji, limit, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
            => SocketUserMessage.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task PinAsync(RequestOptions options = null)
            => SocketUserMessage.PinAsync(options);

        /// <inheritdoc />
        public Task RemoveAllReactionsAsync(RequestOptions options = null)
            => SocketUserMessage.RemoveAllReactionsAsync(options);

        /// <inheritdoc />
        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
            => SocketUserMessage.RemoveReactionAsync(emote, user, options);

        /// <inheritdoc />
        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
            => SocketUserMessage.Resolve(userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);

        /// <inheritdoc />
        public Task UnpinAsync(RequestOptions options = null)
            => SocketUserMessage.UnpinAsync(options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketUserMessage"/> being abstracted.
        /// </summary>
        protected SocketUserMessage SocketUserMessage
            => SocketMessage as SocketUserMessage;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketUserMessage"/> objects.
    /// </summary>
    public static class SocketUserMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketUserMessage"/> to an abstracted <see cref="ISocketUserMessage"/> value.
        /// </summary>
        /// <param name="socketUserMessage">The existing <see cref="SocketUserMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUserMessage"/>.</exception>
        /// <returns>An <see cref="ISocketUserMessage"/> that abstracts <paramref name="socketUserMessage"/>.</returns>
        public static ISocketUserMessage Abstract(this SocketUserMessage socketUserMessage)
            => new SocketUserMessageAbstraction(socketUserMessage);
    }
}
