using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="IRestMessageChannel" />
    public interface IIRestMessageChannel : IMessageChannel
    {
        /// <inheritdoc cref="IRestMessageChannel.GetMessageAsync(ulong, RequestOptions)" />
        Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.GetMessagesAsync(int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.SendFileAsync(string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="IRestMessageChannel.SendMessageAsync(string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.IRestMessageChannel"/>, through the <see cref="IIRestMessageChannel"/> interface.
    /// </summary>
    public class IRestMessageChannelAbstraction : IIRestMessageChannel
    {
        /// <summary>
        /// Constructs a new <see cref="IRestMessageChannelAbstraction"/> around an existing <see cref="Rest.IRestMessageChannel"/>.
        /// </summary>
        /// <param name="iRestMessageChannel">The value to use for <see cref="Rest.IRestMessageChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestMessageChannel"/>.</exception>
        public IRestMessageChannelAbstraction(IRestMessageChannel iRestMessageChannel)
        {
            if (iRestMessageChannel is null)
                throw new ArgumentNullException(nameof(iRestMessageChannel));

            IRestMessageChannel = iRestMessageChannel;
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => IRestMessageChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => IRestMessageChannel.Id;

        /// <inheritdoc />
        public string Name
            => IRestMessageChannel.Name;

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => IRestMessageChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => IRestMessageChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => IRestMessageChannel.EnterTypingState(options);

        /// <inheritdoc />
        public async Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await IRestMessageChannel.GetMessageAsync(id, options))
                .Abstract();

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (IRestMessageChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestMessageChannel.GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await IRestMessageChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (IRestMessageChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestMessageChannel.GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestMessageChannel.GetUsersAsync(mode, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await IRestMessageChannel.SendFileAsync(filePath, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (IRestMessageChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await IRestMessageChannel.SendFileAsync(stream, filename, text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (IRestMessageChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await IRestMessageChannel.SendMessageAsync(text, isTTS, embed, options))
                .Abstract();

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (IRestMessageChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => IRestMessageChannel.TriggerTypingAsync(options);

        /// <summary>
        /// The existing <see cref="Rest.IRestMessageChannel"/> being abstracted.
        /// </summary>
        protected IRestMessageChannel IRestMessageChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="IRestMessageChannel"/> objects.
    /// </summary>
    public static class IRestMessageChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IRestMessageChannel"/> to an abstracted <see cref="IIRestMessageChannel"/> value.
        /// </summary>
        /// <param name="iRestMessageChannel">The existing <see cref="IRestMessageChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestMessageChannel"/>.</exception>
        /// <returns>An <see cref="IIRestMessageChannel"/> that abstracts <paramref name="iRestMessageChannel"/>.</returns>
        public static IIRestMessageChannel Abstract(this IRestMessageChannel iRestMessageChannel)
            => new IRestMessageChannelAbstraction(iRestMessageChannel);
    }
}
