using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketDMChannel" />
    public interface ISocketDMChannel : ISocketChannel, IDMChannel, IISocketPrivateChannel, IISocketMessageChannel
    {
        /// <inheritdoc cref="SocketDMChannel.Users" />
        new IReadOnlyCollection<ISocketUser> Users { get; }

        /// <inheritdoc cref="SocketDMChannel.Recipient" />
        new ISocketUser Recipient { get; }

        /// <inheritdoc cref="SocketDMChannel.CachedMessages" />
        new IReadOnlyCollection<ISocketMessage> CachedMessages { get; }

        /// <inheritdoc cref="SocketDMChannel.GetCachedMessage(ulong)" />
        new ISocketMessage GetCachedMessage(ulong id);

        /// <inheritdoc cref="SocketDMChannel.GetCachedMessages(int)" />
        new IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100);

        /// <inheritdoc cref="SocketDMChannel.GetCachedMessages(ulong, Direction, int)" />
        new IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100);

        /// <inheritdoc cref="SocketDMChannel.GetCachedMessages(IMessage, Direction, int)" />
        new IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100);

        /// <inheritdoc cref="SocketDMChannel.GetMessageAsync(ulong, RequestOptions)" />
        Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.GetMessagesAsync(int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.GetUser(ulong)" />
        new ISocketUser GetUser(ulong id);

        /// <inheritdoc cref="SocketDMChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.SendFileAsync(string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="SocketDMChannel.SendMessageAsync(string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketDMChannel"/>, through the <see cref="ISocketDMChannel"/> interface.
    /// </summary>
    public class SocketDMChannelAbstraction : SocketChannelAbstraction, ISocketDMChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketDMChannelAbstraction"/> around an existing <see cref="WebSocket.SocketDMChannel"/>.
        /// </summary>
        /// <param name="socketDMChannel">The value to use for <see cref="WebSocket.SocketDMChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketDMChannel"/>.</exception>
        public SocketDMChannelAbstraction(SocketDMChannel socketDMChannel)
            : base(socketDMChannel) { }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> CachedMessages
            => SocketDMChannel.CachedMessages
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public ISocketUser Recipient
            => SocketDMChannel.Recipient
                .Abstract();

        /// <inheritdoc />
        IUser IDMChannel.Recipient
            => (SocketDMChannel as IDMChannel).Recipient;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketUser> Recipients
            => (SocketDMChannel as ISocketPrivateChannel).Recipients
                .Select(SocketUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (SocketDMChannel as IPrivateChannel).Recipients;

        /// <inheritdoc />
        public Task CloseAsync(RequestOptions options = null)
            => SocketDMChannel.CloseAsync(options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => SocketDMChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => SocketDMChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => SocketDMChannel.EnterTypingState(options);

        /// <inheritdoc />
        public ISocketMessage GetCachedMessage(ulong id)
            => SocketDMChannel.GetCachedMessage(id)
                .Abstract();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100)
            => SocketDMChannel.GetCachedMessages(limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
            => SocketDMChannel.GetCachedMessages(fromMessageId, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
            => SocketDMChannel.GetCachedMessages(fromMessage, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => SocketDMChannel.GetMessageAsync(id, options);

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketDMChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => SocketDMChannel.GetMessagesAsync(limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketDMChannel as IMessageChannel).GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketDMChannel.GetMessagesAsync(fromMessageId, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketDMChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketDMChannel.GetMessagesAsync(fromMessage, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketDMChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await SocketDMChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (SocketDMChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketDMChannel.SendFileAsync(filePath, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketDMChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketDMChannel.SendFileAsync(stream, filename, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketDMChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketDMChannel.SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketDMChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => SocketDMChannel.TriggerTypingAsync(options);

        /// <inheritdoc cref="SocketDMChannel.ToString" />
        public override string ToString()
            => SocketDMChannel.ToString();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketDMChannel"/> being abstracted.
        /// </summary>
        protected SocketDMChannel SocketDMChannel
            => SocketChannel as SocketDMChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketDMChannel"/> objects.
    /// </summary>
    public static class SocketDMChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketDMChannel"/> to an abstracted <see cref="ISocketDMChannel"/> value.
        /// </summary>
        /// <param name="socketDMChannel">The existing <see cref="SocketDMChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketDMChannel"/>.</exception>
        /// <returns>An <see cref="ISocketDMChannel"/> that abstracts <paramref name="socketDMChannel"/>.</returns>
        public static ISocketDMChannel Abstract(this SocketDMChannel socketDMChannel)
            => new SocketDMChannelAbstraction(socketDMChannel);
    }
}
