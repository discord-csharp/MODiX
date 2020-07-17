using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.Audio;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGroupChannel" />
    public interface IRestGroupChannel : IRestChannel, IGroupChannel, IIRestPrivateChannel, IIRestMessageChannel, IRestAudioChannel
    {
        /// <inheritdoc cref="RestGroupChannel.Recipients" />
        new IReadOnlyCollection<IRestGroupUser> Recipients { get; }

        /// <inheritdoc cref="RestGroupChannel.Users" />
        IReadOnlyCollection<IRestGroupUser> Users { get; }

        /// <inheritdoc cref="RestGroupChannel.GetMessageAsync(ulong, RequestOptions)" />
        new Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGroupChannel.GetMessagesAsync(ulong, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestGroupChannel.GetMessagesAsync(IMessage, Direction, int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestGroupChannel.GetMessagesAsync(int, RequestOptions)" />
        new IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null);

        /// <inheritdoc cref="RestGroupChannel.GetPinnedMessagesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGroupChannel.GetUser(ulong)" />
        IRestUser GetUser(ulong id);

        /// <inheritdoc cref="RestGroupChannel.SendFileAsync(string, string, bool, Embed, RequestOptions, bool, AllowedMentions)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null);

        /// <inheritdoc cref="RestGroupChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions, bool, AllowedMentions)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null);

        /// <inheritdoc cref="RestGroupChannel.SendMessageAsync(string, bool, Embed, RequestOptions, AllowedMentions)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGroupChannel"/>, through the <see cref="IRestGroupChannel"/> interface.
    /// </summary>
    internal class RestGroupChannelAbstraction : RestChannelAbstraction, IRestGroupChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestGroupChannelAbstraction"/> around an existing <see cref="Rest.RestGroupChannel"/>.
        /// </summary>
        /// <param name="restGroupChannel">The value to use for <see cref="Rest.RestGroupChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGroupChannel"/>.</exception>
        public RestGroupChannelAbstraction(RestGroupChannel restGroupChannel)
            : base(restGroupChannel) { }

        /// <inheritdoc />
        public IReadOnlyCollection<IRestGroupUser> Recipients
            => RestGroupChannel.Recipients
                .Select(RestGroupUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IRestUser> IIRestPrivateChannel.Recipients
            => (RestGroupChannel as IRestPrivateChannel).Recipients
                .Select(RestUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (RestGroupChannel as IPrivateChannel).Recipients
                .Select(UserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<IRestGroupUser> Users
            => RestGroupChannel.Users
                .Select(RestGroupUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
            => (RestGroupChannel as IAudioChannel).ConnectAsync(selfDeaf, selfMute, external);

        /// <inheritdoc />
        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
            => RestGroupChannel.DeleteMessageAsync(messageId, options);

        /// <inheritdoc />
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
            => RestGroupChannel.DeleteMessageAsync(message, options);

        /// <inheritdoc />
        public Task DisconnectAsync()
            => (RestGroupChannel as IVoiceChannel).DisconnectAsync();

        /// <inheritdoc />
        public IDisposable EnterTypingState(RequestOptions options = null)
            => RestGroupChannel.EnterTypingState(options);

        /// <inheritdoc />
        public async Task<IRestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => (await RestGroupChannel.GetMessageAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGroupChannel as IMessageChannel).GetMessageAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(int limit = 100, RequestOptions options = null)
            => RestGroupChannel.GetMessagesAsync(limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestGroupChannel as IMessageChannel).GetMessagesAsync(limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, RequestOptions options = null)
            => RestGroupChannel.GetMessagesAsync(fromMessageId, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestGroupChannel as IMessageChannel).GetMessagesAsync(fromMessageId, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, RequestOptions options = null)
            => RestGroupChannel.GetMessagesAsync(fromMessage, dir, limit, options)
                .Select(x => x
                    .Select(RestMessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestGroupChannel as IMessageChannel).GetMessagesAsync(fromMessage, dir, limit, mode, options)
                .Select(x => x
                    .Select(MessageAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => (await RestGroupChannel.GetPinnedMessagesAsync(options))
                .Select(RestMessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options)
            => (await (RestGroupChannel as IMessageChannel).GetPinnedMessagesAsync(options))
                .Select(MessageAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IRestUser GetUser(ulong id)
            => RestGroupChannel.GetUser(id)
                ?.Abstract();

        /// <inheritdoc />
        public Task LeaveAsync(RequestOptions options = null)
            => RestGroupChannel.LeaveAsync(options);

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
            => (await RestGroupChannel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions)
            => (await (RestGroupChannel as IMessageChannel).SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
            => (await RestGroupChannel.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, Embed embed, RequestOptions options, bool isSpoiler, AllowedMentions allowedMentions)
            => (await (RestGroupChannel as IMessageChannel).SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null)
            => (await RestGroupChannel.SendMessageAsync(text, isTTS, embed, options, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        async Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options, AllowedMentions allowedMentions)
            => (await (RestGroupChannel as IMessageChannel).SendMessageAsync(text, isTTS, embed, options, allowedMentions))
                .Abstract();

        /// <inheritdoc />
        public Task TriggerTypingAsync(RequestOptions options = null)
            => RestGroupChannel.TriggerTypingAsync(options);

        /// <inheritdoc cref="RestGroupChannel.ToString" />
        public override string ToString()
            => RestGroupChannel.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestGroupChannel"/> being abstracted.
        /// </summary>
        protected RestGroupChannel RestGroupChannel
            => RestChannel as RestGroupChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGroupChannel"/> objects.
    /// </summary>
    internal static class RestGroupChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGroupChannel"/> to an abstracted <see cref="IRestGroupChannel"/> value.
        /// </summary>
        /// <param name="restGroupChannel">The existing <see cref="RestGroupChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGroupChannel"/>.</exception>
        /// <returns>An <see cref="IRestGroupChannel"/> that abstracts <paramref name="restGroupChannel"/>.</returns>
        public static IRestGroupChannel Abstract(this RestGroupChannel restGroupChannel)
            => new RestGroupChannelAbstraction(restGroupChannel);
    }
}
