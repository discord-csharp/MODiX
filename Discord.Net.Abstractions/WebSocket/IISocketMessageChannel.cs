using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="ISocketMessageChannel" />
    public interface IISocketMessageChannel : IMessageChannel
    {
        /// <inheritdoc cref="ISocketMessageChannel.CachedMessages" />
        IReadOnlyCollection<ISocketMessage> CachedMessages { get; }

        /// <inheritdoc cref="ISocketMessageChannel.GetCachedMessage(ulong)" />
        ISocketMessage GetCachedMessage(ulong id);

        /// <inheritdoc cref="ISocketMessageChannel.GetCachedMessages(int)" />
        IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100);

        /// <inheritdoc cref="ISocketMessageChannel.GetCachedMessages(ulong, Direction, int)" />
        IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100);

        /// <inheritdoc cref="ISocketMessageChannel.GetCachedMessages(IMessage, Direction, int)" />
        IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100);

        /// <inheritdoc cref="ISocketMessageChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="ISocketMessageChannel.SendFileAsync(string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="ISocketMessageChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="ISocketMessageChannel.SendMessageAsync(string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around an <see cref="WebSocket.ISocketMessageChannel"/>, through the <see cref="IISocketMessageChannel"/> interface.
    /// </summary>
    public class ISocketMessageChannelAbstraction : IISocketMessageChannel
    {
        /// <summary>
        /// Constructs a new <see cref="ISocketMessageChannelAbstraction"/> around an existing <see cref="WebSocket.ISocketMessageChannel"/>.
        /// </summary>
        /// <param name="iSocketMessageChannel">The value to use for <see cref="WebSocket.ISocketMessageChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketMessageChannel"/>.</exception>
        public ISocketMessageChannelAbstraction(ISocketMessageChannel iSocketMessageChannel)
        {
            ISocketMessageChannel = iSocketMessageChannel ?? throw new ArgumentNullException(nameof(iSocketMessageChannel));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> CachedMessages
            => ISocketMessageChannel.CachedMessages
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => ISocketMessageChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => ISocketMessageChannel.Id;

        /// <inheritdoc />
        public string Name
            => ISocketMessageChannel.Name;

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => ISocketMessageChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => ISocketMessageChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => ISocketMessageChannel.EnterTypingState(options);

        /// <inheritdoc />
        public ISocketMessage GetCachedMessage(ulong id)
            => ISocketMessageChannel.GetCachedMessage(id)
                .Abstract();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100)
            => ISocketMessageChannel.GetCachedMessages(limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
            => ISocketMessageChannel.GetCachedMessages(fromMessageId, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
            => ISocketMessageChannel.GetCachedMessages(fromMessage, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await ISocketMessageChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (ISocketMessageChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (ISocketMessageChannel as IMessageChannel).GetUsersAsync(mode, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await ISocketMessageChannel.SendFileAsync(filePath, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (ISocketMessageChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await ISocketMessageChannel.SendFileAsync(stream, filename, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (ISocketMessageChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await ISocketMessageChannel.SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (ISocketMessageChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => ISocketMessageChannel.TriggerTypingAsync(options);

        /// <summary>
        /// The existing <see cref="WebSocket.ISocketMessageChannel"/> being abstracted.
        /// </summary>
        protected ISocketMessageChannel ISocketMessageChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="ISocketMessageChannel"/> objects.
    /// </summary>
    public static class ISocketMessageChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ISocketMessageChannel"/> to an abstracted <see cref="IISocketMessageChannel"/> value.
        /// </summary>
        /// <param name="iSocketMessageChannel">The existing <see cref="ISocketMessageChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketMessageChannel"/>.</exception>
        /// <returns>An <see cref="IISocketMessageChannel"/> that abstracts <paramref name="iSocketMessageChannel"/>.</returns>
        public static IISocketMessageChannel Abstract(this ISocketMessageChannel iSocketMessageChannel)
            => new ISocketMessageChannelAbstraction(iSocketMessageChannel);
    }
}
