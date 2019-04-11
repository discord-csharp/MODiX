using System;
using System.Collections.Generic;
using System.IO;
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
    /// Contains extension methods for abstracting <see cref="ISocketMessageChannel"/> objects.
    /// </summary>
    internal static class ISocketMessageChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ISocketMessageChannel"/> to an abstracted <see cref="IISocketMessageChannel"/> value.
        /// </summary>
        /// <param name="iSocketMessageChannel">The existing <see cref="ISocketMessageChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketMessageChannel"/>.</exception>
        /// <returns>An <see cref="IISocketMessageChannel"/> that abstracts <paramref name="iSocketMessageChannel"/>.</returns>
        public static IISocketMessageChannel Abstract(this ISocketMessageChannel iSocketMessageChannel)
            => iSocketMessageChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(iSocketMessageChannel)),
                SocketDMChannel socketDMChannel
                    => SocketDMChannelAbstractionExtensions.Abstract(socketDMChannel) as IISocketMessageChannel,
                SocketGroupChannel socketGroupChannel
                    => SocketGroupChannelAbstractionExtensions.Abstract(socketGroupChannel) as IISocketMessageChannel,
                SocketTextChannel socketTextChannel
                    => SocketTextChannelAbstractionExtensions.Abstract(socketTextChannel) as IISocketMessageChannel,
                _
                    => throw new NotSupportedException($"{nameof(ISocketMessageChannel)} type {iSocketMessageChannel.GetType().FullName} is not supported")
            };
    }
}
