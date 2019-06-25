using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.Audio;
using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketGroupChannel" />
    public interface ISocketGroupChannel : ISocketChannel, IGroupChannel, IISocketPrivateChannel, IISocketMessageChannel, ISocketAudioChannel
    {
        /// <inheritdoc cref="SocketGroupChannel.Recipients" />
        new IReadOnlyCollection<ISocketGroupUser> Recipients { get; }

        /// <inheritdoc cref="SocketGroupChannel.Users" />
        new IReadOnlyCollection<ISocketGroupUser> Users { get; }

        /// <inheritdoc cref="SocketGroupChannel.ConnectAsync" />
        Task<IAudioClient> ConnectAsync();

        /// <inheritdoc cref="SocketGroupChannel.GetMessageAsync(ulong, RequestOptions)" />
        Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="SocketGroupChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketGroupChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketGroupChannel.GetMessagesAsync(int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketGroupChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="SocketGroupChannel.GetUser(ulong)" />
        new ISocketGroupUser GetUser(ulong id);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketGroupChannel"/>, through the <see cref="ISocketGroupChannel"/> interface.
    /// </summary>
    internal class SocketGroupChannelAbstraction : SocketChannelAbstraction, ISocketGroupChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketGroupChannelAbstraction"/> around an existing <see cref="WebSocket.SocketGroupChannel"/>.
        /// </summary>
        /// <param name="socketGroupChannel">The value to use for <see cref="WebSocket.SocketGroupChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGroupChannel"/>.</exception>
        public SocketGroupChannelAbstraction(SocketGroupChannel socketGroupChannel)
            : base(socketGroupChannel) { }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> CachedMessages
            => SocketGroupChannel.CachedMessages
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGroupUser> Recipients
            => SocketGroupChannel.Recipients
                .Select(SocketGroupUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<ISocketUser> IISocketPrivateChannel.Recipients
            => (SocketGroupChannel as ISocketPrivateChannel).Recipients
                .Select(SocketUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (SocketGroupChannel as IPrivateChannel).Recipients
                .Select(UserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        new public IReadOnlyCollection<ISocketGroupUser> Users
            => SocketGroupChannel.Users
                .Select(SocketGroupUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<IAudioClient> ConnectAsync()
            => SocketGroupChannel.ConnectAsync();

        /// <inheritdoc />
        public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
            => (SocketGroupChannel as IAudioChannel).ConnectAsync(selfDeaf, selfMute, external);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => SocketGroupChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => SocketGroupChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public Task DisconnectAsync()
            => (SocketGroupChannel as IAudioChannel).DisconnectAsync();

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => SocketGroupChannel.EnterTypingState(options);

        /// <inheritdoc />
        public ISocketMessage GetCachedMessage(ulong id)
            => SocketGroupChannel.GetCachedMessage(id)
                ?.Abstract();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100)
            => SocketGroupChannel.GetCachedMessages(limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
            => SocketGroupChannel.GetCachedMessages(fromMessageId, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
            => SocketGroupChannel.GetCachedMessages(fromMessage, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await SocketGroupChannel.GetMessageAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (SocketGroupChannel as IMessageChannel).GetMessageAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => SocketGroupChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketGroupChannel as IMessageChannel).GetMessagesAsync(limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketGroupChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketGroupChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketGroupChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketGroupChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await SocketGroupChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (await (SocketGroupChannel as IMessageChannel).GetPinnedMessagesAsync(options))
                .Select(MessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        new public ISocketGroupUser GetUser(ulong id)
            => SocketGroupChannel.GetUser(id)
                ?.Abstract();

        /// <inheritdoc />
        public Task LeaveAsync(RequestOptions options = null)
            => SocketGroupChannel.LeaveAsync(options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketGroupChannel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler)
            => (await (SocketGroupChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketGroupChannel.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler)
            => (await (SocketGroupChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketGroupChannel.SendMessageAsync(text, isTTS, embed, options));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (await (SocketGroupChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => SocketGroupChannel.TriggerTypingAsync(options);

        /// <inheritdoc cref="SocketGroupChannel.ToString" />
        public override string ToString()
            => SocketGroupChannel.ToString();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketGroupChannel"/> being abstracted.
        /// </summary>
        protected SocketGroupChannel SocketGroupChannel
            => SocketChannel as SocketGroupChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketGroupChannel"/> objects.
    /// </summary>
    internal static class SocketGroupChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketGroupChannel"/> to an abstracted <see cref="ISocketGroupChannel"/> value.
        /// </summary>
        /// <param name="socketGroupChannel">The existing <see cref="SocketGroupChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGroupChannel"/>.</exception>
        /// <returns>An <see cref="ISocketGroupChannel"/> that abstracts <paramref name="socketGroupChannel"/>.</returns>
        public static ISocketGroupChannel Abstract(this SocketGroupChannel socketGroupChannel)
            => new SocketGroupChannelAbstraction(socketGroupChannel);
    }
}
