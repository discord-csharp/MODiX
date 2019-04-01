using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestTextChannel" />
    public interface IRestTextChannel : IRestGuildChannel, IIRestMessageChannel, ITextChannel
    {
        /// <inheritdoc cref="RestTextChannel.CreateWebhookAsync(string, Stream, RequestOptions)" />
        new Task<IRestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetCategoryAsync(RequestOptions)" />
        Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetMessageAsync(ulong, RequestOptions)" />
        new Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetMessagesAsync(int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetUserAsync(ulong, RequestOptions)" />
        Task<IRestGuildUser> GetUserAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetUsersAsync(RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestGuildUser>> GetUsersAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetWebhookAsync(ulong, RequestOptions)" />
        new Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.GetWebhooksAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.SendFileAsync(string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="RestTextChannel.SendMessageAsync(string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestTextChannel"/>, through the <see cref="IRestTextChannel"/> interface.
    /// </summary>
    public class RestTextChannelAbstraction : RestGuildChannelAbstraction, IRestTextChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestTextChannelAbstraction"/> around an existing <see cref="Rest.RestTextChannel"/>.
        /// </summary>
        /// <param name="restTextChannel">The value to use for <see cref="Rest.RestTextChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restTextChannel"/>.</exception>
        public RestTextChannelAbstraction(RestTextChannel restTextChannel)
            : base(restTextChannel) { }

        /// <inheritdoc />
        public ulong? CategoryId
            => RestTextChannel.CategoryId;

        /// <inheritdoc />
        public bool IsNsfw
            => RestTextChannel.IsNsfw;

        /// <inheritdoc />
        public string Mention
            => RestTextChannel.Mention;

        /// <inheritdoc />
        public int SlowModeInterval
            => RestTextChannel.SlowModeInterval;

        /// <inheritdoc />
        public string Topic
            => RestTextChannel.Topic;

        /// <inheritdoc />
        public async Task<IRestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
            => (await RestTextChannel.CreateWebhookAsync(name, avatar, options))
                .Abstract();

        /// <inheritdoc />
        Task<IWebhook> ITextChannel.CreateWebhookAsync(string name, Stream avatar, RequestOptions options)
            => (RestTextChannel as ITextChannel).CreateWebhookAsync(name, avatar, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => RestTextChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => RestTextChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
            => RestTextChannel.DeleteMessagesAsync(messages, options);

        /// <inheritdoc />
        public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
            => RestTextChannel.DeleteMessagesAsync(messageIds, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => RestTextChannel.EnterTypingState(options);

        /// <inheritdoc />
        public Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null)
            => RestTextChannel.GetCategoryAsync(options);

        /// <inheritdoc />
        public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestTextChannel as INestedChannel).GetCategoryAsync(mode, options);

        /// <inheritdoc />
        public async Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await RestTextChannel.GetMessageAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestTextChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => RestTextChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestTextChannel as IMessageChannel).GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => RestTextChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestTextChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => RestTextChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestTextChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await RestTextChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (RestTextChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public async Task<IRestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
            => (await RestTextChannel.GetUserAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestGuildUser>> GetUsersAsync(RequestOptions options = null)
            => RestTextChannel.GetUsersAsync(options)
                .Select(x => x
                    .Select(RestGuildUserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => (await RestTextChannel.GetWebhookAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        Task<IWebhook> ITextChannel.GetWebhookAsync(ulong id, RequestOptions options)
            => (RestTextChannel as ITextChannel).GetWebhookAsync(id, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null)
            => (await RestTextChannel.GetWebhooksAsync(options))
                .Select(RestWebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IWebhook>> ITextChannel.GetWebhooksAsync(RequestOptions options)
            => (RestTextChannel as ITextChannel).GetWebhooksAsync(options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
            => RestTextChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestTextChannel.SendFileAsync(filePath, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestTextChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestTextChannel.SendFileAsync(stream, filename, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestTextChannel.SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        public Task SyncPermissionsAsync(RequestOptions options = null)
            => RestTextChannel.SyncPermissionsAsync(options);

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestTextChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => (RestTextChannel as ITextChannel).TriggerTypingAsync(options);

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestTextChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <summary>
        /// The existing <see cref="Rest.RestTextChannel"/> being abstracted.
        /// </summary>
        protected RestTextChannel RestTextChannel
            => RestGuildChannel as RestTextChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestTextChannel"/> objects.
    /// </summary>
    public static class RestTextChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestTextChannel"/> to an abstracted <see cref="IRestTextChannel"/> value.
        /// </summary>
        /// <param name="restTextChannel">The existing <see cref="RestTextChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restTextChannel"/>.</exception>
        /// <returns>An <see cref="IRestTextChannel"/> that abstracts <paramref name="restTextChannel"/>.</returns>
        public static IRestTextChannel Abstract(this RestTextChannel restTextChannel)
            => new RestTextChannelAbstraction(restTextChannel);
    }
}
