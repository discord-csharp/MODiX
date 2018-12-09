using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestDMChannel" />
    public interface IRestDMChannel : IRestChannel, IDMChannel, IMessageChannel, IPrivateChannel, IRestPrivateChannel, IRestMessageChannel
    {
        /// <inheritdoc cref="RestDMChannel.Users" />
        IReadOnlyCollection<IRestUser> Users { get; }

        /// <inheritdoc cref="RestDMChannel.CurrentUser" />
        IRestUser CurrentUser { get; }

        /// <inheritdoc cref="RestDMChannel.Recipient" />
        new IRestUser Recipient { get; }

        /// <inheritdoc cref="IRestPrivateChannel.Recipients" />
        new IReadOnlyCollection<IRestUser> Recipients { get; }

        /// <inheritdoc cref="RestDMChannel.GetMessageAsync(ulong, RequestOptions)" />
        new Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.GetMessagesAsync(int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.GetUser(ulong)" />
        IRestUser GetUser(ulong id);

        /// <inheritdoc cref="RestDMChannel.SendFileAsync(string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null);

        /// <inheritdoc cref="RestDMChannel.SendMessageAsync(string, bool, Embed, RequestOptions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestDMChannel"/>, through the <see cref="IRestDMChannel"/> interface.
    /// </summary>
    public class RestDMChannelAbstraction : RestChannelAbstraction, IRestDMChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestDMChannelAbstraction"/> around an existing <see cref="Rest.RestDMChannel"/>.
        /// </summary>
        /// <param name="restDMChannel">The value to use for <see cref="Rest.RestDMChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restDMChannel"/>.</exception>
        public RestDMChannelAbstraction(RestDMChannel restDMChannel)
            : base(restDMChannel) { }

        /// <inheritdoc />
        public IRestUser CurrentUser
            => RestDMChannel.CurrentUser
                .Abstract();

        /// <inheritdoc />
        public IRestUser Recipient
            => RestDMChannel.Recipient
                .Abstract();

        /// <inheritdoc />
        IUser IDMChannel.Recipient
            => RestDMChannel.Recipient;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestUser> Recipients
            => (RestDMChannel as IRestPrivateChannel).Recipients
                .Select(RestUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<RestUser> IRestPrivateChannel.Recipients
            => (RestDMChannel as IRestPrivateChannel).Recipients;

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (RestDMChannel as IPrivateChannel).Recipients;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestUser> Users
            => RestDMChannel.Users
                .Select(RestUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task CloseAsync(RequestOptions options = null)
            => RestDMChannel.CloseAsync(options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => RestDMChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => RestDMChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => RestDMChannel.EnterTypingState(options);

        /// <inheritdoc />
        public async Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await RestDMChannel.GetMessageAsync(id, options)).Abstract();

        /// <inheritdoc />
        Task<RestMessage> IRestMessageChannel.GetMessageAsync(ulong id, RequestOptions options)
            => RestDMChannel.GetMessageAsync(id, options);

        /// <inheritdoc />
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestDMChannel as IMessageChannel).GetMessageAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => RestDMChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<RestMessage>> IRestMessageChannel.GetMessagesAsync(int limit, RequestOptions options)
            => RestDMChannel.GetMessagesAsync(limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => RestDMChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<RestMessage>> IRestMessageChannel.GetMessagesAsync(ulong fromMessageId, Direction dir, int limit, RequestOptions options)
            => RestDMChannel.GetMessagesAsync(fromMessageId, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => RestDMChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<RestMessage>> IRestMessageChannel.GetMessagesAsync(IMessage fromMessage, Direction dir, int limit, RequestOptions options)
            => RestDMChannel.GetMessagesAsync(fromMessage, dir, limit, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestDMChannel as IMessageChannel).GetMessagesAsync(limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestDMChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestDMChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await RestDMChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<RestMessage>> IRestMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => RestDMChannel.GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (RestDMChannel as IMessageChannel).GetPinnedMessagesAsync(options);

        /// <inheritdoc />
        public IRestUser GetUser(ulong id)
            => RestDMChannel.GetUser(id).Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestDMChannel.SendFileAsync(filePath, text, isTTS, embed, options)).Abstract();

        /// <inheritdoc />
        Task<RestUserMessage> IRestMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => RestDMChannel.SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestDMChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestDMChannel.SendFileAsync(stream, filename, text, isTTS, embed, options)).Abstract();

        /// <inheritdoc />
        Task<RestUserMessage> IRestMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => RestDMChannel.SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestDMChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => (await RestDMChannel.SendMessageAsync(text, isTTS, embed, options)).Abstract();

        /// <inheritdoc />
        Task<RestUserMessage> IRestMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => RestDMChannel.SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options)
            => (RestDMChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options);

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => RestDMChannel.TriggerTypingAsync(options);

        /// <inheritdoc cref="RestDMChannel.ToString()" />
        public override string ToString()
            => RestDMChannel.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestChannel"/> being abstracted.
        /// </summary>
        protected RestDMChannel RestDMChannel
            => RestChannel as RestDMChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestDMChannel"/> objects.
    /// </summary>
    public static class RestDMChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestDMChannel"/> to an abstracted <see cref="IRestDMChannel"/> value.
        /// </summary>
        /// <param name="restDMChannel">The existing <see cref="RestDMChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restDMChannel"/>.</exception>
        /// <returns>An <see cref="IRestDMChannel"/> that abstracts <paramref name="restDMChannel"/>.</returns>
        public static IRestDMChannel Abstract(this RestDMChannel restDMChannel)
            => new RestDMChannelAbstraction(restDMChannel);
    }
}
