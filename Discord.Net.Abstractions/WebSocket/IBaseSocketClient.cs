using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.Rest;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="BaseSocketClient" />
    public interface IBaseSocketClient : IBaseDiscordClient, IDiscordClient, IDisposable
    {
        /// <inheritdoc cref="BaseSocketClient.VoiceRegions" />
        IReadOnlyCollection<IRestVoiceRegion> VoiceRegions { get; }

        /// <inheritdoc cref="BaseSocketClient.PrivateChannels" />
        IReadOnlyCollection<IISocketPrivateChannel> PrivateChannels { get; }

        /// <inheritdoc cref="BaseSocketClient.Guilds" />
        IReadOnlyCollection<ISocketGuild> Guilds { get; }

        /// <inheritdoc cref="BaseSocketClient.CurrentUser" />
        new ISocketSelfUser CurrentUser { get; }

        /// <inheritdoc cref="BaseSocketClient.Activity" />
        IActivity Activity { get; }

        /// <inheritdoc cref="BaseSocketClient.Status" />
        UserStatus Status { get; }

        /// <inheritdoc cref="BaseSocketClient.Latency" />
        int Latency { get; }

        /// <inheritdoc cref="BaseSocketClient.RoleCreated" />
        event Func<ISocketRole, Task> RoleCreated;

        /// <inheritdoc cref="BaseSocketClient.RoleDeleted" />
        event Func<ISocketRole, Task> RoleDeleted;

        /// <inheritdoc cref="BaseSocketClient.RoleUpdated" />
        event Func<ISocketRole, ISocketRole, Task> RoleUpdated;

        /// <inheritdoc cref="BaseSocketClient.JoinedGuild" />
        event Func<ISocketGuild, Task> JoinedGuild;

        /// <inheritdoc cref="BaseSocketClient.LeftGuild" />
        event Func<ISocketGuild, Task> LeftGuild;

        /// <inheritdoc cref="BaseSocketClient.ReactionsCleared" />
        event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, Task> ReactionsCleared;

        /// <inheritdoc cref="BaseSocketClient.GuildAvailable" />
        event Func<ISocketGuild, Task> GuildAvailable;

        /// <inheritdoc cref="BaseSocketClient.GuildMembersDownloaded" />
        event Func<ISocketGuild, Task> GuildMembersDownloaded;

        /// <inheritdoc cref="BaseSocketClient.GuildUpdated" />
        event Func<ISocketGuild, ISocketGuild, Task> GuildUpdated;

        /// <inheritdoc cref="BaseSocketClient.UserJoined" />
        event Func<ISocketGuildUser, Task> UserJoined;

        /// <inheritdoc cref="BaseSocketClient.UserLeft" />
        event Func<ISocketGuildUser, Task> UserLeft;

        /// <inheritdoc cref="BaseSocketClient.UserBanned" />
        event Func<ISocketUser, ISocketGuild, Task> UserBanned;

        /// <inheritdoc cref="BaseSocketClient.UserUnbanned" />
        event Func<ISocketUser, ISocketGuild, Task> UserUnbanned;

        /// <inheritdoc cref="BaseSocketClient.UserUpdated" />
        event Func<ISocketUser, ISocketUser, Task> UserUpdated;

        /// <inheritdoc cref="BaseSocketClient.GuildMemberUpdated" />
        event Func<ISocketGuildUser, ISocketGuildUser, Task> GuildMemberUpdated;

        /// <inheritdoc cref="BaseSocketClient.UserVoiceStateUpdated" />
        event Func<ISocketUser, ISocketVoiceState, ISocketVoiceState, Task> UserVoiceStateUpdated;

        /// <inheritdoc cref="BaseSocketClient.VoiceServerUpdated" />
        event Func<ISocketVoiceServer, Task> VoiceServerUpdated;

        /// <inheritdoc cref="BaseSocketClient.CurrentUserUpdated" />
        event Func<ISocketSelfUser, ISocketSelfUser, Task> CurrentUserUpdated;

        /// <inheritdoc cref="BaseSocketClient.UserIsTyping" />
        event Func<ISocketUser, IISocketMessageChannel, Task> UserIsTyping;

        /// <inheritdoc cref="BaseSocketClient.GuildUnavailable" />
        event Func<ISocketGuild, Task> GuildUnavailable;

        /// <inheritdoc cref="BaseSocketClient.ReactionRemoved" />
        event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionRemoved;

        /// <inheritdoc cref="BaseSocketClient.MessageReceived" />
        event Func<ISocketMessage, Task> MessageReceived;

        /// <inheritdoc cref="BaseSocketClient.MessageUpdated" />
        event Func<ICacheable<IMessage, ulong>, ISocketMessage, IISocketMessageChannel, Task> MessageUpdated;

        /// <inheritdoc cref="BaseSocketClient.ReactionAdded" />
        event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionAdded;

        /// <inheritdoc cref="BaseSocketClient.ChannelCreated" />
        event Func<ISocketChannel, Task> ChannelCreated;

        /// <inheritdoc cref="BaseSocketClient.ChannelDestroyed" />
        event Func<ISocketChannel, Task> ChannelDestroyed;

        /// <inheritdoc cref="BaseSocketClient.RecipientRemoved" />
        event Func<ISocketGroupUser, Task> RecipientRemoved;

        /// <inheritdoc cref="BaseSocketClient.RecipientAdded" />
        event Func<ISocketGroupUser, Task> RecipientAdded;

        /// <inheritdoc cref="BaseSocketClient.MessageDeleted" />
        event Func<ICacheable<IMessage, ulong>, IISocketMessageChannel, Task> MessageDeleted;

        /// <inheritdoc cref="BaseSocketClient.ChannelUpdated" />
        event Func<ISocketChannel, ISocketChannel, Task> ChannelUpdated;

        /// <inheritdoc cref="BaseSocketClient.CreateGuildAsync(string, IVoiceRegion, Stream, RequestOptions)" />
        new Task<IRestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null);

        /// <inheritdoc cref="BaseSocketClient.DownloadUsersAsync(IEnumerable{IGuild})" />
        Task DownloadUsersAsync(IEnumerable<IGuild> guilds);

        /// <inheritdoc cref="BaseSocketClient.GetApplicationInfoAsync(RequestOptions)" />
        new Task<IRestApplication> GetApplicationInfoAsync(RequestOptions options = null);

        /// <inheritdoc cref="BaseSocketClient.GetChannel(ulong)" />
        ISocketChannel GetChannel(ulong id);

        /// <inheritdoc cref="BaseSocketClient.GetConnectionsAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null);

        /// <inheritdoc cref="BaseSocketClient.GetGuild(ulong)" />
        ISocketGuild GetGuild(ulong id);

        /// <inheritdoc cref="BaseSocketClient.GetInviteAsync(string, RequestOptions)" />
        new Task<IRestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null);

        /// <inheritdoc cref="BaseSocketClient.GetUser(string, string)" />
        ISocketUser GetUser(ulong id);

        /// <inheritdoc cref="BaseSocketClient.GetUser(string, string)" />
        ISocketUser GetUser(string username, string discriminator);

        /// <inheritdoc cref="BaseSocketClient.GetVoiceRegion(string)" />
        IRestVoiceRegion GetVoiceRegion(string id);

        /// <inheritdoc cref="BaseSocketClient.SetActivityAsync(IActivity)" />
        Task SetActivityAsync(IActivity activity);

        /// <inheritdoc cref="BaseSocketClient.SetGameAsync(string, string, ActivityType)" />
        Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing);

        /// <inheritdoc cref="BaseSocketClient.SetStatusAsync(UserStatus)" />
        Task SetStatusAsync(UserStatus status);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.BaseSocketClient"/>, through the <see cref="IBaseSocketClient"/> interface.
    /// </summary>
    public abstract class BaseSocketClientAbstraction : BaseDiscordClientAbstraction, IBaseSocketClient
    {
        /// <summary>
        /// Constructs a new <see cref="BaseSocketClientAbstraction"/> around an existing <see cref="WebSocket.BaseSocketClient"/>.
        /// </summary>
        /// <param name="baseSocketClient">The value to use for <see cref="WebSocket.BaseSocketClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseSocketClient"/>.</exception>
        protected BaseSocketClientAbstraction(BaseSocketClient baseSocketClient)
            : base(baseSocketClient)
        {
            baseSocketClient.ChannelCreated += x => ChannelCreated?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.ChannelDestroyed += x => ChannelDestroyed?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.ChannelUpdated += (x, y) => ChannelUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.CurrentUserUpdated += (x, y) => CurrentUserUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.GuildAvailable += x => GuildAvailable?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.GuildMembersDownloaded += x => GuildMembersDownloaded?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.GuildMemberUpdated += (x, y) => GuildMemberUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.GuildUnavailable += x => GuildUnavailable?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.GuildUpdated += (x, y) => GuildUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.JoinedGuild += x => JoinedGuild?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.LeftGuild += x => LeftGuild?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.MessageReceived += x => MessageReceived?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            // TODO: Workaround for https://github.com/RogueException/Discord.Net/issues/1208
            baseSocketClient.MessageUpdated += (x, y, z) => !(y is null)
                ? (MessageUpdated?.InvokeAsync(x.Abstract(), y.Abstract(), z.Abstract()) ?? Task.CompletedTask)
                : Task.CompletedTask;
            baseSocketClient.MessageDeleted += (x, y) => MessageDeleted?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.ReactionAdded += (x, y, z) => ReactionAdded?.InvokeAsync(x.Abstract(), y.Abstract(), z.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.ReactionRemoved += (x, y, z) => ReactionRemoved?.InvokeAsync(x.Abstract(), y.Abstract(), z.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.ReactionsCleared += (x, y) => ReactionsCleared?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.RecipientAdded += x => RecipientAdded?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.RecipientRemoved += x => RecipientRemoved?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.RoleCreated += x => RoleCreated?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.RoleDeleted += x => RoleDeleted?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.RoleUpdated += (x, y) => RoleUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserBanned += (x, y) => UserBanned?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserIsTyping += (x, y) => UserIsTyping?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserJoined += x => UserJoined?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserLeft += x => UserLeft?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserUnbanned += (x, y) => UserUnbanned?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserUpdated += (x, y) => UserUpdated?.InvokeAsync(x.Abstract(), y.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.UserVoiceStateUpdated += (x, y, z) => UserVoiceStateUpdated?.InvokeAsync(x.Abstract(), y.Abstract(), z.Abstract()) ?? Task.CompletedTask;
            baseSocketClient.VoiceServerUpdated += x => VoiceServerUpdated?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
        }

        /// <inheritdoc />
        public IActivity Activity
            => BaseSocketClient.Activity;

        /// <inheritdoc />
        new public ISocketSelfUser CurrentUser
            => BaseSocketClient.CurrentUser
                .Abstract();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGuild> Guilds
            => BaseSocketClient.Guilds
                .Select(SocketGuildAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public int Latency
            => BaseSocketClient.Latency;

        /// <inheritdoc />
        public IReadOnlyCollection<IISocketPrivateChannel> PrivateChannels
            => BaseSocketClient.PrivateChannels
                .Select(ISocketPrivateChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public UserStatus Status
            => BaseSocketClient.Status;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestVoiceRegion> VoiceRegions
            => BaseSocketClient.VoiceRegions
                .Select(RestVoiceRegionAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public event Func<ISocketChannel, Task> ChannelCreated;

        /// <inheritdoc />
        public event Func<ISocketChannel, Task> ChannelDestroyed;

        /// <inheritdoc />
        public event Func<ISocketChannel, ISocketChannel, Task> ChannelUpdated;

        /// <inheritdoc />
        public event Func<ISocketSelfUser, ISocketSelfUser, Task> CurrentUserUpdated;

        /// <inheritdoc />
        public event Func<ISocketGuild, Task> GuildAvailable;

        /// <inheritdoc />
        public event Func<ISocketGuild, Task> GuildMembersDownloaded;

        /// <inheritdoc />
        public event Func<ISocketGuildUser, ISocketGuildUser, Task> GuildMemberUpdated;

        /// <inheritdoc />
        public event Func<ISocketGuild, Task> GuildUnavailable;

        /// <inheritdoc />
        public event Func<ISocketGuild, ISocketGuild, Task> GuildUpdated;

        /// <inheritdoc />
        public event Func<ISocketGuild, Task> JoinedGuild;

        /// <inheritdoc />
        public event Func<ISocketGuild, Task> LeftGuild;

        /// <inheritdoc />
        public event Func<ISocketMessage, Task> MessageReceived;

        /// <inheritdoc />
        public event Func<ICacheable<IMessage, ulong>, IISocketMessageChannel, Task> MessageDeleted;

        /// <inheritdoc />
        public event Func<ICacheable<IMessage, ulong>, ISocketMessage, IISocketMessageChannel, Task> MessageUpdated;

        /// <inheritdoc />
        public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionAdded;

        /// <inheritdoc />
        public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionRemoved;

        /// <inheritdoc />
        public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, Task> ReactionsCleared;

        /// <inheritdoc />
        public event Func<ISocketGroupUser, Task> RecipientAdded;

        /// <inheritdoc />
        public event Func<ISocketGroupUser, Task> RecipientRemoved;

        /// <inheritdoc />
        public event Func<ISocketRole, Task> RoleCreated;

        /// <inheritdoc />
        public event Func<ISocketRole, Task> RoleDeleted;

        /// <inheritdoc />
        public event Func<ISocketRole, ISocketRole, Task> RoleUpdated;

        /// <inheritdoc />
        public event Func<ISocketUser, ISocketGuild, Task> UserBanned;

        /// <inheritdoc />
        public event Func<ISocketUser, IISocketMessageChannel, Task> UserIsTyping;

        /// <inheritdoc />
        public event Func<ISocketGuildUser, Task> UserJoined;

        /// <inheritdoc />
        public event Func<ISocketGuildUser, Task> UserLeft;

        /// <inheritdoc />
        public event Func<ISocketUser, ISocketGuild, Task> UserUnbanned;

        /// <inheritdoc />
        public event Func<ISocketUser, ISocketUser, Task> UserUpdated;

        /// <inheritdoc />
        public event Func<ISocketUser, ISocketVoiceState, ISocketVoiceState, Task> UserVoiceStateUpdated;

        /// <inheritdoc />
        public event Func<ISocketVoiceServer, Task> VoiceServerUpdated;

        /// <inheritdoc />
        new public async Task<IRestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
            => RestGuildAbstractionExtensions.Abstract(
                await BaseSocketClient.CreateGuildAsync(name, region, jpegIcon, options));

        /// <inheritdoc />
        public Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
            => BaseSocketClient.DownloadUsersAsync(guilds);

        /// <inheritdoc />
        new public async Task<IRestApplication> GetApplicationInfoAsync(RequestOptions options)
            => RestApplicationAbstractionExtensions.Abstract(
                await BaseSocketClient.GetApplicationInfoAsync(options));

        /// <inheritdoc />
        public ISocketChannel GetChannel(ulong id)
            => BaseSocketClient.GetChannel(id)
                .Abstract();

        /// <inheritdoc />
        public ISocketGuild GetGuild(ulong id)
            => BaseSocketClient.GetGuild(id)
                .Abstract();

        /// <inheritdoc />
        new public async Task<IRestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options)
            => RestInviteMetadataAbstractionExtensions.Abstract(
                await BaseSocketClient.GetInviteAsync(inviteId, options));

        /// <inheritdoc />
        public ISocketUser GetUser(ulong id)
            => BaseSocketClient.GetUser(id)
                .Abstract();

        /// <inheritdoc />
        public ISocketUser GetUser(string username, string discriminator)
            => BaseSocketClient.GetUser(username, discriminator)
                .Abstract();

        /// <inheritdoc />
        public IRestVoiceRegion GetVoiceRegion(string id)
            => RestVoiceRegionAbstractionExtensions.Abstract(
                BaseSocketClient.GetVoiceRegion(id));

        /// <inheritdoc />
        public Task SetActivityAsync(IActivity activity)
            => BaseSocketClient.SetActivityAsync(activity);

        /// <inheritdoc />
        public Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
            => BaseSocketClient.SetGameAsync(name, streamUrl, type);

        /// <inheritdoc />
        public Task SetStatusAsync(UserStatus status)
            => BaseSocketClient.SetStatusAsync(status);

        /// <summary>
        /// The existing <see cref="WebSocket.BaseSocketClient"/> being abstracted.
        /// </summary>
        protected BaseSocketClient BaseSocketClient
            => BaseDiscordClient as BaseSocketClient;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="BaseSocketClient"/> objects.
    /// </summary>
    public static class BaseSocketClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="BaseSocketClient"/> to an abstracted <see cref="IBaseSocketClient"/> value.
        /// </summary>
        /// <param name="baseSocketClient">The existing <see cref="BaseSocketClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="baseSocketClient"/>.</exception>
        /// <returns>An <see cref="IBaseSocketClient"/> that abstracts <paramref name="baseSocketClient"/>.</returns>
        public static IBaseSocketClient Abstract(this BaseSocketClient baseSocketClient)
            => baseSocketClient switch
            {
                null
                    => throw new ArgumentNullException(nameof(baseSocketClient)),
                DiscordSocketClient discordSocketClient
                    => discordSocketClient.Abstract() as IBaseSocketClient,
                DiscordShardedClient discordShardedClient
                    => discordShardedClient.Abstract() as IBaseSocketClient,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(BaseSocketClient)} type {baseSocketClient.GetType().Name}")
            };
    }
}
