using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketTextChannel" />
    public interface ISocketTextChannel : ISocketGuildChannel, ITextChannel, IISocketMessageChannel
    {
        /// <inheritdoc cref="SocketTextChannel.Category" />
        ICategoryChannel Category { get; }

        /// <inheritdoc cref="SocketTextChannel.CreateWebhookAsync(string, Stream, RequestOptions)" />
        new Task<IRestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetMessageAsync(ulong, RequestOptions)" />
        Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetMessagesAsync(int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetWebhookAsync(ulong, RequestOptions)" />
        new Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="SocketTextChannel.GetWebhooksAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketTextChannel"/>, through the <see cref="ISocketTextChannel"/> interface.
    /// </summary>
    public class SocketTextChannelAbstraction : SocketGuildChannelAbstraction, ISocketTextChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketTextChannelAbstraction"/> around an existing <see cref="WebSocket.SocketTextChannel"/>.
        /// </summary>
        /// <param name="socketTextChannel">The value to use for <see cref="WebSocket.SocketTextChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketTextChannel"/>.</exception>
        public SocketTextChannelAbstraction(SocketTextChannel socketTextChannel)
            : base(socketTextChannel) { }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> CachedMessages
            => SocketTextChannel.CachedMessages
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public ICategoryChannel Category
            => SocketTextChannel.Category;

        /// <inheritdoc />
        public ulong? CategoryId
            => SocketTextChannel.CategoryId;

        /// <inheritdoc />
        public bool IsNsfw
            => SocketTextChannel.IsNsfw;

        /// <inheritdoc />
        public string Mention
            => SocketTextChannel.Mention;

        /// <inheritdoc />
        public int SlowModeInterval
            => SocketTextChannel.SlowModeInterval;

        /// <inheritdoc />
        public string Topic
            => SocketTextChannel.Topic;

        /// <inheritdoc />
        public Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
            => SocketTextChannel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, options);

        /// <inheritdoc />
        public async Task<IRestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
            => (await SocketTextChannel.CreateWebhookAsync(name, avatar, options))
                .Abstract();

        /// <inheritdoc />
        Task<IWebhook> ITextChannel.CreateWebhookAsync(string name, Stream avatar, RequestOptions options)
            => (SocketTextChannel as ITextChannel).CreateWebhookAsync(name, avatar, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => SocketTextChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => SocketTextChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
            => SocketTextChannel.DeleteMessagesAsync(messages, options);

        /// <inheritdoc />
        public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
            => SocketTextChannel.DeleteMessagesAsync(messageIds, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => SocketTextChannel.EnterTypingState(options);

        /// <inheritdoc />
        public ISocketMessage GetCachedMessage(ulong id)
            => SocketTextChannel.GetCachedMessage(id)
                .Abstract();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(int limit = 100)
            => SocketTextChannel.GetCachedMessages(limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(ulong fromMessageId, Direction dir, int limit = 100)
            => SocketTextChannel.GetCachedMessages(fromMessageId, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketMessage> GetCachedMessages(IMessage fromMessage, Direction dir, int limit = 100)
            => SocketTextChannel.GetCachedMessages(fromMessage, dir, limit)
                .Select(SocketMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as INestedChannel).GetCategoryAsync(mode, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => SocketTextChannel.GetInvitesAsync(options);

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => SocketTextChannel.GetMessageAsync(id, options);

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(fromMessageId, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(fromMessage, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await SocketTextChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (SocketTextChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public async Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => (await SocketTextChannel.GetWebhookAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        Task<IWebhook> ITextChannel.GetWebhookAsync(ulong id, RequestOptions options)
            => (SocketTextChannel as ITextChannel).GetWebhookAsync(id, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null)
            => (await SocketTextChannel.GetWebhooksAsync(options))
                .Select(RestWebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IWebhook>> ITextChannel.GetWebhooksAsync(RequestOptions options)
            => (SocketTextChannel as ITextChannel).GetWebhooksAsync(options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
            => SocketTextChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketTextChannel.SendFileAsync(filePath, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketTextChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketTextChannel.SendFileAsync(stream, filename, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketTextChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await SocketTextChannel.SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (SocketTextChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        public Task SyncPermissionsAsync(RequestOptions options = null)
            => SocketTextChannel.SyncPermissionsAsync(options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).TriggerTypingAsync(options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketTextChannel"/> being abstracted.
        /// </summary>
        protected SocketTextChannel SocketTextChannel
            => SocketGuildChannel as SocketTextChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketTextChannel"/> objects.
    /// </summary>
    public static class SocketTextChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketTextChannel"/> to an abstracted <see cref="ISocketTextChannel"/> value.
        /// </summary>
        /// <param name="socketTextChannel">The existing <see cref="SocketTextChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketTextChannel"/>.</exception>
        /// <returns>An <see cref="ISocketTextChannel"/> that abstracts <paramref name="socketTextChannel"/>.</returns>
        public static ISocketTextChannel Abstract(this SocketTextChannel socketTextChannel)
            => new SocketTextChannelAbstraction(socketTextChannel);
    }
}
