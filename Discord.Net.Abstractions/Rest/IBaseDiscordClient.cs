using System;
using System.Collections.Generic;
using System.IO;
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
    public abstract class BaseDiscordClientAbstraction : IBaseDiscordClient
    {
        /// <summary>
        /// Constructs a new <see cref="BaseDiscordClientAbstraction"/> around an existing <see cref="Rest.BaseDiscordClient"/>.
        /// </summary>
        /// <param name="baseDiscordClient">The value to use for <see cref="Rest.BaseDiscordClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseDiscordClient"/>.</exception>
        protected BaseDiscordClientAbstraction(BaseDiscordClient baseDiscordClient)
        {
            if (baseDiscordClient is null)
                throw new ArgumentNullException(nameof(baseDiscordClient));

            BaseDiscordClient = baseDiscordClient;

            baseDiscordClient.Log += x => Log.InvokeAsync(x.Abstract());
        }

        /// <inheritdoc />
        public ConnectionState ConnectionState
            => (BaseDiscordClient as IDiscordClient).ConnectionState;

        /// <inheritdoc />
        public ISelfUser CurrentUser
            => BaseDiscordClient.CurrentUser;

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
        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).CreateGuildAsync(name, region, jpegIcon, options);

        /// <inheritdoc />
        public void Dispose()
            => BaseDiscordClient.Dispose();

        /// <inheritdoc />
        public Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetApplicationInfoAsync(options);

        /// <inheritdoc />
        public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetChannelAsync(id, mode, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetConnectionsAsync(options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetDMChannelsAsync(mode, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetGroupChannelsAsync(mode, options);

        /// <inheritdoc />
        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetGuildAsync(id, mode, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetGuildsAsync(mode, options);

        /// <inheritdoc />
        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetInviteAsync(inviteId, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetPrivateChannelsAsync(mode, options);

        /// <inheritdoc />
        public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetRecommendedShardCountAsync(options);

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetUserAsync(username, discriminator, options);

        /// <inheritdoc />
        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetVoiceRegionAsync(id, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetVoiceRegionsAsync(options);

        /// <inheritdoc />
        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => (BaseDiscordClient as IDiscordClient).GetWebhookAsync(id, options);

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
    public static class BaseDiscordClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="BaseDiscordClient"/> to an abstracted <see cref="IBaseDiscordClient"/> value.
        /// </summary>
        /// <param name="baseDiscordClient">The existing <see cref="BaseDiscordClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseDiscordClient"/>.</exception>
        /// <returns>An <see cref="IBaseDiscordClient"/> that abstracts <paramref name="baseDiscordClient"/>.</returns>
        public static IBaseDiscordClient Abstract(this BaseDiscordClient baseDiscordClient)
            => (baseDiscordClient is null) ? throw new ArgumentNullException(nameof(baseDiscordClient))
                : (baseDiscordClient is DiscordRestClient discordRestClient) ? discordRestClient.Abstract() as IBaseDiscordClient
                : (baseDiscordClient is BaseSocketClient baseSocketClient) ? baseSocketClient.Abstract() as IBaseDiscordClient
                : throw new NotSupportedException($"Unable to abstract {nameof(BaseDiscordClient)} type {baseDiscordClient.GetType().Name}");
    }
}
