using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

namespace Discord.Rest
{
    /// <inheritdoc cref="DiscordRestClient" />
    public interface IDiscordRestClient : IBaseDiscordClient
    {
        /// <inheritdoc cref="DiscordRestClient.CurrentUser" />
        new IRestSelfUser CurrentUser { get; }

        /// <inheritdoc cref="DiscordRestClient.CreateGuildAsync(string, IVoiceRegion, Stream, RequestOptions)" />
        new Task<IRestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetApplicationInfoAsync(RequestOptions)" />
        new Task<IRestApplication> GetApplicationInfoAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetChannelAsync(ulong, RequestOptions)" />
        Task<IRestChannel> GetChannelAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetConnectionsAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetDMChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestDMChannel>> GetDMChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGroupChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestGroupChannel>> GetGroupChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildAsync(ulong, RequestOptions)" />
        Task<IRestGuild> GetGuildAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildEmbedAsync(ulong, RequestOptions)" />
        Task<IRestGuildEmbed> GetGuildEmbedAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestGuild>> GetGuildsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildSummariesAsync(ulong, int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestUserGuild>> GetGuildSummariesAsync(ulong fromGuildId, int limit, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildSummariesAsync(RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestUserGuild>> GetGuildSummariesAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetGuildUserAsync(ulong, ulong, RequestOptions)" />
        Task<IRestGuildUser> GetGuildUserAsync(ulong guildId, ulong id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetInviteAsync(string, RequestOptions)" />
        new Task<IRestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetPrivateChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IIRestPrivateChannel>> GetPrivateChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetUserAsync(ulong, RequestOptions)" />
        Task<IRestUser> GetUserAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetVoiceRegionsAsync(string, RequestOptions)" />
        new Task<IRestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetVoiceRegionsAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null);

        /// <inheritdoc cref="DiscordRestClient.GetWebhookAsync(ulong, RequestOptions)" />
        new Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.DiscordRestClient"/>, through the <see cref="IDiscordRestClient"/> interface.
    /// </summary>
    internal class DiscordRestClientAbstraction : BaseDiscordClientAbstraction, IDiscordRestClient
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordRestClientAbstraction"/> around an existing <see cref="Rest.DiscordRestClient"/>.
        /// </summary>
        /// <param name="discordRestClient">The value to use for <see cref="Rest.DiscordRestClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordRestClient"/>.</exception>
        public DiscordRestClientAbstraction(DiscordRestClient discordRestClient)
            : base(discordRestClient) { }

        /// <inheritdoc />
        new public IRestSelfUser CurrentUser
            => DiscordRestClient.CurrentUser
                .Abstract();

        /// <inheritdoc />
        new public async Task<IRestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
            => (await DiscordRestClient.CreateGuildAsync(name, region, jpegIcon, options))
                .Abstract();

        /// <inheritdoc />
        new public async Task<IRestApplication> GetApplicationInfoAsync(RequestOptions options)
            => (await DiscordRestClient.GetApplicationInfoAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestChannel> GetChannelAsync(ulong id, RequestOptions options = null)
            => (await DiscordRestClient.GetChannelAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestDMChannel>> GetDMChannelsAsync(RequestOptions options = null)
            => (await DiscordRestClient.GetDMChannelsAsync(options))
                .Select(RestDMChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestGroupChannel>> GetGroupChannelsAsync(RequestOptions options = null)
            => (await DiscordRestClient.GetGroupChannelsAsync(options))
                .Select(RestGroupChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestGuild> GetGuildAsync(ulong id, RequestOptions options = null)
            => (await DiscordRestClient.GetGuildAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        [Obsolete("This endpoint is deprecated, please use GetGuildWidgetAsync instead.")]
        public async Task<IRestGuildEmbed> GetGuildEmbedAsync(ulong id, RequestOptions options = null)
            => (await DiscordRestClient.GetGuildEmbedAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestGuild>> GetGuildsAsync(RequestOptions options = null)
            => (await DiscordRestClient.GetGuildsAsync(options))
                .Select(RestGuildAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestUserGuild>> GetGuildSummariesAsync(ulong fromGuildId, int limit, RequestOptions options = null)
            => DiscordRestClient.GetGuildSummariesAsync(fromGuildId, limit, options)
                .Select(x => x
                    .Select(RestUserGuildAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestUserGuild>> GetGuildSummariesAsync(RequestOptions options = null)
            => DiscordRestClient.GetGuildSummariesAsync(options)
                .Select(x => x
                    .Select(RestUserGuildAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IRestGuildUser> GetGuildUserAsync(ulong guildId, ulong id, RequestOptions options = null)
            => (await DiscordRestClient.GetGuildUserAsync(guildId, id, options))
                ?.Abstract();

        /// <inheritdoc />
        new public async Task<IRestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options)
            => (await DiscordRestClient.GetInviteAsync(inviteId, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIRestPrivateChannel>> GetPrivateChannelsAsync(RequestOptions options = null)
            => (await DiscordRestClient.GetPrivateChannelsAsync(options))
                .Select(IRestPrivateChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestUser> GetUserAsync(ulong id, RequestOptions options = null)
            => (await DiscordRestClient.GetUserAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        new public async Task<IRestVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options)
            => (await DiscordRestClient.GetVoiceRegionAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        new public async Task<IReadOnlyCollection<IRestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options)
            => (await DiscordRestClient.GetVoiceRegionsAsync(options))
                .Select(RestVoiceRegionAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        new public async Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options)
            => (await DiscordRestClient.GetWebhookAsync(id, options))
                ?.Abstract();

        /// <summary>
        /// The existing <see cref="Rest.DiscordRestClient"/> being abstracted.
        /// </summary>
        protected DiscordRestClient DiscordRestClient
            => BaseDiscordClient as DiscordRestClient;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="DiscordRestClient"/> objects.
    /// </summary>
    public static class DiscordRestClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="DiscordRestClient"/> to an abstracted <see cref="IDiscordRestClient"/> value.
        /// </summary>
        /// <param name="discordRestClient">The existing <see cref="DiscordRestClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordRestClient"/>.</exception>
        /// <returns>An <see cref="IDiscordRestClient"/> that abstracts <paramref name="discordRestClient"/>.</returns>
        public static IDiscordRestClient Abstract(this DiscordRestClient discordRestClient)
            => discordRestClient switch
            {
                null
                    => throw new ArgumentNullException(nameof(discordRestClient)),
                DiscordSocketRestClient discordSocketRestClient
                    => DiscordSocketRestClientAbstractionExtensions.Abstract(discordSocketRestClient) as IDiscordRestClient,
                _
                    => new DiscordRestClientAbstraction(discordRestClient) as IDiscordRestClient
            };
    }
}
