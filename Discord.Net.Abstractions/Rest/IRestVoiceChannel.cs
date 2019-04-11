using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord.Audio;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestVoiceChannel" />
    public interface IRestVoiceChannel : IRestGuildChannel, IVoiceChannel, IRestAudioChannel
    {
        /// <inheritdoc cref="RestVoiceChannel.GetCategoryAsync(RequestOptions)" />
        Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestVoiceChannel"/>, through the <see cref="IRestVoiceChannel"/> interface.
    /// </summary>
    internal class RestVoiceChannelAbstraction : RestGuildChannelAbstraction, IRestVoiceChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestVoiceChannelAbstraction"/> around an existing <see cref="Rest.RestVoiceChannel"/>.
        /// </summary>
        /// <param name="restVoiceChannel">The value to use for <see cref="Rest.RestVoiceChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restVoiceChannel"/>.</exception>
        public RestVoiceChannelAbstraction(RestVoiceChannel restVoiceChannel)
            : base(restVoiceChannel) { }

        /// <inheritdoc />
        public int Bitrate
            => RestVoiceChannel.Bitrate;

        /// <inheritdoc />
        public ulong? CategoryId
            => RestVoiceChannel.CategoryId;

        /// <inheritdoc />
        public int? UserLimit
            => RestVoiceChannel.UserLimit;

        /// <inheritdoc />
        public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
            => (RestVoiceChannel as IVoiceChannel).ConnectAsync(selfDeaf, selfMute, external);

        /// <inheritdoc />
        public async Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
            => (await RestVoiceChannel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, options))
                .Abstract();

        /// <inheritdoc />
        public Task DisconnectAsync()
            => (RestVoiceChannel as IVoiceChannel).DisconnectAsync();

        /// <inheritdoc />
        public async Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null)
            => (await RestVoiceChannel.GetCategoryAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestVoiceChannel as IVoiceChannel).GetCategoryAsync(mode, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => (await RestVoiceChannel.GetInvitesAsync(options))
                .Select(InviteMetadataAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
            => RestVoiceChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task SyncPermissionsAsync(RequestOptions options = null)
            => RestVoiceChannel.SyncPermissionsAsync(options);

        /// <summary>
        /// The existing <see cref="Rest.RestVoiceChannel"/> being abstracted.
        /// </summary>
        protected RestVoiceChannel RestVoiceChannel
            => RestGuildChannel as RestVoiceChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestVoiceChannel"/> objects.
    /// </summary>
    internal static class RestVoiceChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestVoiceChannel"/> to an abstracted <see cref="IRestVoiceChannel"/> value.
        /// </summary>
        /// <param name="restVoiceChannel">The existing <see cref="RestVoiceChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restVoiceChannel"/>.</exception>
        /// <returns>An <see cref="IRestVoiceChannel"/> that abstracts <paramref name="restVoiceChannel"/>.</returns>
        public static IRestVoiceChannel Abstract(this RestVoiceChannel restVoiceChannel)
            => new RestVoiceChannelAbstraction(restVoiceChannel);
    }
}
