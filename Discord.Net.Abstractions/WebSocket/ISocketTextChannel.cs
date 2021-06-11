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
    internal class SocketTextChannelAbstraction : SocketGuildChannelAbstraction, ISocketTextChannel
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
            => SocketTextChannel.Category
                .Abstract();

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
        public async Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
            => (await SocketTextChannel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
            => RestWebhookAbstractionExtensions.Abstract(
                await SocketTextChannel.CreateWebhookAsync(name, avatar, options));

        /// <inheritdoc />
        async Task<IWebhook> ITextChannel.CreateWebhookAsync(string name, Stream avatar, RequestOptions options)
            => (await (SocketTextChannel as ITextChannel).CreateWebhookAsync(name, avatar, options))
                .Abstract();

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
                ?.Abstract();

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
        public async Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (SocketTextChannel as INestedChannel).GetCategoryAsync(mode, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => (await SocketTextChannel.GetInvitesAsync(options))
                .Select(InviteMetadataAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await SocketTextChannel.GetMessageAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (SocketTextChannel as IMessageChannel).GetMessageAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => SocketTextChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketTextChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await SocketTextChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (await (SocketTextChannel as IMessageChannel).GetPinnedMessagesAsync(options))
                .Select(MessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
        {
            var restWebhook = await SocketTextChannel.GetWebhookAsync(id, options);

            return (restWebhook is null)
                ? null
                : RestWebhookAbstractionExtensions.Abstract(restWebhook);
        }

        /// <inheritdoc />
        async Task<IWebhook> ITextChannel.GetWebhookAsync(ulong id, RequestOptions options)
            => (await (SocketTextChannel as ITextChannel).GetWebhookAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null)
            => (await SocketTextChannel.GetWebhooksAsync(options))
                .Select(RestWebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IWebhook>> ITextChannel.GetWebhooksAsync(RequestOptions options)
            => (await (SocketTextChannel as ITextChannel).GetWebhooksAsync(options))
                .Select(WebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
            => SocketTextChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public async Task<IUserMessage> ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options = null)
            => (await SocketTextChannel.ModifyMessageAsync(messageId, func, options))
                ?.Abstract();

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.ModifyMessageAsync(ulong messageId, Action<MessageProperties> func, RequestOptions options)
            => (await (SocketTextChannel as IMessageChannel).ModifyMessageAsync(messageId, func, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketTextChannel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageReference messageReference)
            => (await (SocketTextChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketTextChannel.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions, MessageReference messageReference)
            => (await (SocketTextChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
            => RestUserMessageAbstractionExtensions.Abstract(
                await SocketTextChannel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference));

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageReference messageReference)
            => (await (SocketTextChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference))
                .Abstract();

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
    internal static class SocketTextChannelAbstractionExtensions
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
