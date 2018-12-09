using System;
using System.Threading.Tasks;

using Discord.Audio;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketVoiceChannel" />
    public interface ISocketVoiceChannel : ISocketGuildChannel, IVoiceChannel, INestedChannel, IAudioChannel, ISocketAudioChannel
    {
        /// <inheritdoc cref="SocketVoiceChannel.Category" />
        ICategoryChannel Category { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketVoiceChannel"/>, through the <see cref="ISocketVoiceChannel"/> interface.
    /// </summary>
    public class SocketVoiceChannelAbstraction : SocketGuildChannelAbstraction, ISocketVoiceChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketVoiceChannelAbstraction"/> around an existing <see cref="WebSocket.SocketVoiceChannel"/>.
        /// </summary>
        /// <param name="socketVoiceChannel">The value to use for <see cref="WebSocket.SocketVoiceChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceChannel"/>.</exception>
        public SocketVoiceChannelAbstraction(SocketVoiceChannel socketVoiceChannel)
            : base(socketVoiceChannel) { }

        /// <inheritdoc />
        public int Bitrate
            => SocketVoiceChannel.Bitrate;

        /// <inheritdoc />
        public ICategoryChannel Category
            => SocketVoiceChannel.Category;

        /// <inheritdoc />
        public ulong? CategoryId
            => SocketVoiceChannel.CategoryId;

        /// <inheritdoc />
        public int? UserLimit
            => SocketVoiceChannel.UserLimit;

        /// <inheritdoc />
        public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
            => SocketVoiceChannel.ConnectAsync(selfDeaf, selfMute, external);

        /// <inheritdoc />
        public Task DisconnectAsync()
            => SocketVoiceChannel.DisconnectAsync();

        /// <inheritdoc />
        Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
            => (SocketVoiceChannel as INestedChannel).GetCategoryAsync(mode, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
            => SocketVoiceChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task SyncPermissionsAsync(RequestOptions options = null)
            => SocketVoiceChannel.SyncPermissionsAsync(options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketVoiceChannel"/> being abstracted.
        /// </summary>
        protected SocketVoiceChannel SocketVoiceChannel
            => SocketGuildChannel as SocketVoiceChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketVoiceChannel"/> objects.
    /// </summary>
    public static class SocketVoiceChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketVoiceChannel"/> to an abstracted <see cref="ISocketVoiceChannel"/> value.
        /// </summary>
        /// <param name="socketVoiceChannel">The existing <see cref="SocketVoiceChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceChannel"/>.</exception>
        /// <returns>An <see cref="ISocketVoiceChannel"/> that abstracts <paramref name="socketVoiceChannel"/>.</returns>
        public static ISocketVoiceChannel Abstract(this SocketVoiceChannel socketVoiceChannel)
            => new SocketVoiceChannelAbstraction(socketVoiceChannel);
    }
}
