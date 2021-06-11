using System;
using System.Collections.Generic;
using System.IO;
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

        /// <inheritdoc cref="IRestMessageChannel.SendFileAsync(string, string, bool, Embed, RequestOptions, bool, AllowedMentions, MessageReference)" />
        new Task<IRestUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null);

        /// <inheritdoc cref="IRestMessageChannel.SendFileAsync(Stream, string, string, bool, Embed, RequestOptions, bool, AllowedMentions, MessageReference)" />
        new Task<IRestUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null);

        /// <inheritdoc cref="IRestMessageChannel.SendMessageAsync(string, bool, Embed, RequestOptions, AllowedMentions, MessageReference)" />
        new Task<IRestUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null);
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="IRestMessageChannel"/> objects.
    /// </summary>
    internal static class IRestMessageChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IRestMessageChannel"/> to an abstracted <see cref="IIRestMessageChannel"/> value.
        /// </summary>
        /// <param name="iRestMessageChannel">The existing <see cref="IRestMessageChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestMessageChannel"/>.</exception>
        /// <returns>An <see cref="IIRestMessageChannel"/> that abstracts <paramref name="iRestMessageChannel"/>.</returns>
        public static IIRestMessageChannel Abstract(this IRestMessageChannel iRestMessageChannel)
            => iRestMessageChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(iRestMessageChannel)),
                RestDMChannel restDMChannel
                    => RestDMChannelAbstractionExtensions.Abstract(restDMChannel) as IIRestMessageChannel,
                RestGroupChannel restGroupChannel
                    => RestGroupChannelAbstractionExtensions.Abstract(restGroupChannel) as IIRestMessageChannel,
                RestTextChannel restTextChannel
                    => RestTextChannelAbstractionExtensions.Abstract(restTextChannel) as IIRestMessageChannel,
                _
                    => throw new NotSupportedException($"{nameof(IRestMessageChannel)} type {iRestMessageChannel.GetType().FullName} is not supported")
            };
    }
}
