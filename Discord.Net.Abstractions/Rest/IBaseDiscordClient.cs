using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

namespace Discord.Rest
{
    /// <inheritdoc cref="BaseDiscordClient" />
    public interface IBaseDiscordClient : IDiscordClient, IDisposable
    {
        /// <inheritdoc cref="BaseDiscordClient.LoginState" />
        LoginState LoginState { get; }

        /// <inheritdoc cref="BaseDiscordClient.Log" />
        event Func<ILogMessage, Task> Log;

        /// <inheritdoc cref="BaseDiscordClient.LoggedIn" />
        event Func<Task> LoggedIn;

        /// <inheritdoc cref="BaseDiscordClient.LoggedOut" />
        event Func<Task> LoggedOut;

        /// <inheritdoc cref="BaseDiscordClient.LoginAsync(TokenType, string, bool)" />
        Task LoginAsync(TokenType tokenType, string token, bool validateToken = true);

        /// <inheritdoc cref="BaseDiscordClient.LogoutAsync" />
        Task LogoutAsync();
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.BaseDiscordClient"/>, through the <see cref="IBaseDiscordClient"/> interface.
    /// </summary>
    internal abstract class BaseDiscordClientAbstraction : IBaseDiscordClient
    {
        /// <summary>
        /// Constructs a new <see cref="BaseDiscordClientAbstraction"/> around an existing <see cref="Rest.BaseDiscordClient"/>.
        /// </summary>
        /// <param name="baseDiscordClient">The value to use for <see cref="Rest.BaseDiscordClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseDiscordClient"/>.</exception>
        protected BaseDiscordClientAbstraction(BaseDiscordClient baseDiscordClient)
        {
            BaseDiscordClient = baseDiscordClient ?? throw new ArgumentNullException(nameof(baseDiscordClient));

            baseDiscordClient.Log += x => Log?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
        }

        /// <inheritdoc />
        public ConnectionState ConnectionState
            => (BaseDiscordClient as IDiscordClient).ConnectionState;

        /// <inheritdoc />
        public ISelfUser CurrentUser
            => BaseDiscordClient.CurrentUser
                .Abstract();

        /// <inheritdoc />
        public LoginState LoginState
            => BaseDiscordClient.LoginState;

        /// <inheritdoc />
        public TokenType TokenType
            => BaseDiscordClient.TokenType;

        /// <inheritdoc />
        public event Func<ILogMessage, Task> Log;

        /// <inheritdoc />
        public event Func<Task> LoggedIn
        {
            add { BaseDiscordClient.LoggedIn += value; }
            remove { BaseDiscordClient.LoggedIn -= value; }
        }

        /// <inheritdoc />
        public event Func<Task> LoggedOut
        {
            add { BaseDiscordClient.LoggedOut += value; }
            remove { BaseDiscordClient.LoggedOut -= value; }
        }

        /// <inheritdoc />
        public async Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).CreateGuildAsync(name, region, jpegIcon, options))
                .Abstract();

        /// <inheritdoc />
        public void Dispose()
            => BaseDiscordClient.Dispose();

        /// <inheritdoc />
        public async Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetApplicationInfoAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetChannelAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetConnectionsAsync(options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetDMChannelsAsync(mode, options))
                .Select(DMChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetGroupChannelsAsync(mode, options))
                .Select(GroupChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetGuildAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetGuildsAsync(mode, options))
                .Select(GuildAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetInviteAsync(inviteId, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetPrivateChannelsAsync(mode, options))
                .Select(PrivateChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetRecommendedShardCountAsync(options);

        /// <inheritdoc />
        public async Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetUserAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetUserAsync(username, discriminator, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetVoiceRegionAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetVoiceRegionsAsync(options))
                .Select(VoiceRegionAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => (await (BaseDiscordClient as IDiscordClient).GetWebhookAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
            => BaseDiscordClient.LoginAsync(tokenType, token, validateToken);

        /// <inheritdoc />
        public Task LogoutAsync()
            => BaseDiscordClient.LogoutAsync();

        /// <inheritdoc />
        public Task StartAsync()
            => (BaseDiscordClient as IDiscordClient).StartAsync();

        /// <inheritdoc />
        public Task StopAsync()
            => (BaseDiscordClient as IDiscordClient).StopAsync();

        /// <summary>
        /// The existing <see cref="Rest.BaseDiscordClient"/> being abstracted.
        /// </summary>
        protected BaseDiscordClient BaseDiscordClient { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="BaseDiscordClient"/> objects.
    /// </summary>
    internal static class BaseDiscordClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="BaseDiscordClient"/> to an abstracted <see cref="IBaseDiscordClient"/> value.
        /// </summary>
        /// <param name="baseDiscordClient">The existing <see cref="BaseDiscordClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseDiscordClient"/>.</exception>
        /// <returns>An <see cref="IBaseDiscordClient"/> that abstracts <paramref name="baseDiscordClient"/>.</returns>
        public static IBaseDiscordClient Abstract(this BaseDiscordClient baseDiscordClient)
            => baseDiscordClient switch
            {
                null
                    => throw new ArgumentNullException(nameof(baseDiscordClient)),
                DiscordRestClient discordRestClient
                    => DiscordRestClientAbstractionExtensions.Abstract(discordRestClient) as IBaseDiscordClient,
                BaseSocketClient baseSocketClient
                    => BaseSocketClientAbstractionExtensions.Abstract(baseSocketClient) as IBaseDiscordClient,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(BaseDiscordClient)} type {baseDiscordClient.GetType().Name}")
            };
    }
}
