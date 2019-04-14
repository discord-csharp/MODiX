using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Services.Core;

using Modix.Common.Test.Mocks;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class DiscordSocketListeningBehaviorTests
    {
        #region Fakes

        #pragma warning disable CS0067 // Unused members (no shit, that's how interfaces work)
        private class FakeDiscordSocketClient : IDiscordSocketClient
        {
            public IReadOnlyCollection<ISocketGroupChannel> GroupChannels
                => throw new NotImplementedException();

            public IReadOnlyCollection<ISocketDMChannel> DMChannels
                => throw new NotImplementedException();

            public IDiscordSocketRestClient Rest
                => throw new NotImplementedException();

            public int ShardId
                => throw new NotImplementedException();

            public IReadOnlyCollection<IRestVoiceRegion> VoiceRegions
                => throw new NotImplementedException();

            public IReadOnlyCollection<IISocketPrivateChannel> PrivateChannels
                => throw new NotImplementedException();

            public IReadOnlyCollection<ISocketGuild> Guilds
                => throw new NotImplementedException();

            public ISocketSelfUser CurrentUser
                => throw new NotImplementedException();

            public IActivity Activity
                => throw new NotImplementedException();

            public UserStatus Status
                => throw new NotImplementedException();

            public int Latency
                => throw new NotImplementedException();

            public LoginState LoginState
                => throw new NotImplementedException();

            public ConnectionState ConnectionState
                => throw new NotImplementedException();

            public TokenType TokenType
                => throw new NotImplementedException();

            ISelfUser IDiscordClient.CurrentUser
                => throw new NotImplementedException();

            public event Func<Exception, Task> Disconnected;

            public event Func<Task> Connected;

            public event Func<int, int, Task> LatencyUpdated;

            public event Func<Task> Ready
            {
                add => FakeReadyEvent.AddHandler(value);
                remove => FakeReadyEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent FakeReadyEvent
                = new FakeAsyncEvent();

            public event Func<ISocketRole, Task> RoleCreated;

            public event Func<ISocketRole, Task> RoleDeleted;

            public event Func<ISocketRole, ISocketRole, Task> RoleUpdated;

            public event Func<ISocketGuild, Task> JoinedGuild
            {
                add => FakeJoinedGuildEvent.AddHandler(value);
                remove => FakeJoinedGuildEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketGuild> FakeJoinedGuildEvent
                = new FakeAsyncEvent<ISocketGuild>();

            public event Func<ISocketGuild, Task> LeftGuild;

            public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, Task> ReactionsCleared;

            public event Func<ISocketGuild, Task> GuildAvailable
            {
                add => FakeGuildAvailableEvent.AddHandler(value);
                remove => FakeGuildAvailableEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketGuild> FakeGuildAvailableEvent
                = new FakeAsyncEvent<ISocketGuild>();

            public event Func<ISocketGuild, Task> GuildMembersDownloaded;

            public event Func<ISocketGuild, ISocketGuild, Task> GuildUpdated;

            public event Func<ISocketGuildUser, Task> UserJoined
            {
                add => FakeUserJoinedEvent.AddHandler(value);
                remove => FakeUserJoinedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketGuildUser> FakeUserJoinedEvent
                = new FakeAsyncEvent<ISocketGuildUser>();

            public event Func<ISocketGuildUser, Task> UserLeft
            {
                add => FakeUserLeftEvent.AddHandler(value);
                remove => FakeUserLeftEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketGuildUser> FakeUserLeftEvent
                = new FakeAsyncEvent<ISocketGuildUser>();

            public event Func<ISocketUser, ISocketGuild, Task> UserBanned
            {
                add => FakeUserBannedEvent.AddHandler(value);
                remove => FakeUserBannedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketUser, ISocketGuild> FakeUserBannedEvent
                = new FakeAsyncEvent<ISocketUser, ISocketGuild>();

            public event Func<ISocketUser, ISocketGuild, Task> UserUnbanned;

            public event Func<ISocketUser, ISocketUser, Task> UserUpdated;

            public event Func<ISocketGuildUser, ISocketGuildUser, Task> GuildMemberUpdated;

            public event Func<ISocketUser, ISocketVoiceState, ISocketVoiceState, Task> UserVoiceStateUpdated;

            public event Func<ISocketVoiceServer, Task> VoiceServerUpdated;

            public event Func<ISocketSelfUser, ISocketSelfUser, Task> CurrentUserUpdated;

            public event Func<ISocketUser, IISocketMessageChannel, Task> UserIsTyping;

            public event Func<ISocketGuild, Task> GuildUnavailable;

            public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionRemoved
            {
                add => FakeReactionRemovedEvent.AddHandler(value);
                remove => FakeReactionRemovedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction> FakeReactionRemovedEvent
                = new FakeAsyncEvent<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction>();

            public event Func<ISocketMessage, Task> MessageReceived
            {
                add => FakeMessageReceivedEvent.AddHandler(value);
                remove => FakeMessageReceivedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketMessage> FakeMessageReceivedEvent
                = new FakeAsyncEvent<ISocketMessage>();

            public event Func<ICacheable<IMessage, ulong>, ISocketMessage, IISocketMessageChannel, Task> MessageUpdated
            {
                add => FakeMessageUpdatedEvent.AddHandler(value);
                remove => FakeMessageUpdatedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ICacheable<IMessage, ulong>, ISocketMessage, IISocketMessageChannel> FakeMessageUpdatedEvent
                = new FakeAsyncEvent<ICacheable<IMessage, ulong>, ISocketMessage, IISocketMessageChannel>();

            public event Func<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction, Task> ReactionAdded
            {
                add => FakeReactionAddedEvent.AddHandler(value);
                remove => FakeReactionAddedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction> FakeReactionAddedEvent
                = new FakeAsyncEvent<ICacheable<IUserMessage, ulong>, IISocketMessageChannel, ISocketReaction>();

            public event Func<ISocketChannel, Task> ChannelCreated
            {
                add => FakeChannelCreatedEvent.AddHandler(value);
                remove => FakeChannelCreatedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketChannel> FakeChannelCreatedEvent
                = new FakeAsyncEvent<ISocketChannel>();

            public event Func<ISocketChannel, Task> ChannelDestroyed;

            public event Func<ISocketGroupUser, Task> RecipientRemoved;

            public event Func<ISocketGroupUser, Task> RecipientAdded;

            public event Func<ICacheable<IMessage, ulong>, IISocketMessageChannel, Task> MessageDeleted
            {
                add => FakeMessageDeletedEvent.AddHandler(value);
                remove => FakeMessageDeletedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ICacheable<IMessage, ulong>, IISocketMessageChannel> FakeMessageDeletedEvent
                = new FakeAsyncEvent<ICacheable<IMessage, ulong>, IISocketMessageChannel>();

            public event Func<ISocketChannel, ISocketChannel, Task> ChannelUpdated
            {
                add => FakeChannelUpdatedEvent.AddHandler(value);
                remove => FakeChannelUpdatedEvent.RemoveHandler(value);
            }
            public readonly FakeAsyncEvent<ISocketChannel, ISocketChannel> FakeChannelUpdatedEvent
                = new FakeAsyncEvent<ISocketChannel, ISocketChannel>();

            public event Func<ILogMessage, Task> Log;

            public event Func<Task> LoggedIn;

            public event Func<Task> LoggedOut;

            public Task<IRestGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
                => throw new NotImplementedException();

            public void Dispose()
                => throw new NotImplementedException();

            public Task DownloadUsersAsync(IEnumerable<IGuild> guilds)
                => throw new NotImplementedException();

            public Task<IRestApplication> GetApplicationInfoAsync(RequestOptions options = null)
                => throw new NotImplementedException();

            public ISocketChannel GetChannel(ulong id)
                => throw new NotImplementedException();

            public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public ISocketGuild GetGuild(ulong id)
                => throw new NotImplementedException();

            public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IRestInviteMetadata> GetInviteAsync(string inviteId, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
                => throw new NotImplementedException();

            public ISocketUser GetUser(ulong id)
                => throw new NotImplementedException();

            public ISocketUser GetUser(string username, string discriminator)
                => throw new NotImplementedException();

            public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null)
                => throw new NotImplementedException();

            public IRestVoiceRegion GetVoiceRegion(string id)
                => throw new NotImplementedException();

            public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
                => throw new NotImplementedException();

            public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
                => throw new NotImplementedException();

            public Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
                => throw new NotImplementedException();

            public Task LogoutAsync()
                => throw new NotImplementedException();

            public Task SetActivityAsync(IActivity activity)
                => throw new NotImplementedException();

            public Task SetGameAsync(string name, string streamUrl = null, ActivityType type = ActivityType.Playing)
                => throw new NotImplementedException();

            public Task SetStatusAsync(UserStatus status)
                => throw new NotImplementedException();

            public Task StartAsync()
                => throw new NotImplementedException();

            public Task StopAsync()
                => throw new NotImplementedException();

            Task<IGuild> IDiscordClient.CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
                => throw new NotImplementedException();

            Task<IApplication> IDiscordClient.GetApplicationInfoAsync(RequestOptions options)
                => throw new NotImplementedException();

            Task<IInvite> IDiscordClient.GetInviteAsync(string inviteId, RequestOptions options)
                => throw new NotImplementedException();
        }
        #pragma warning restore CS0067

        #endregion Fakes

        #region StartAsync() Tests

        [Test]
        public void StartAsync_Always_CompletesImmediately()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            uut.StartAsync().IsCompleted.ShouldBeTrue();

            fakeDiscordSocketClient.FakeChannelCreatedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeChannelUpdatedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeGuildAvailableEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeJoinedGuildEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeMessageDeletedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeMessageUpdatedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeReactionAddedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeReactionRemovedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeReadyEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeUserBannedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeUserJoinedEvent.Handlers.Count.ShouldBe(1);
            fakeDiscordSocketClient.FakeUserLeftEvent.Handlers.Count.ShouldBe(1);
        }

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public async Task StopAsync_Always_CompletesImmediately()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();
            uut.StopAsync().IsCompleted.ShouldBeTrue();

            fakeDiscordSocketClient.FakeChannelCreatedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeChannelUpdatedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeGuildAvailableEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeJoinedGuildEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeMessageDeletedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeMessageReceivedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeMessageUpdatedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeReactionAddedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeReactionRemovedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeReadyEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeUserBannedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeUserJoinedEvent.Handlers.ShouldBeEmpty();
            fakeDiscordSocketClient.FakeUserLeftEvent.Handlers.ShouldBeEmpty();
        }

        #endregion StopAsync() Tests

        #region DiscordSocketClient.ChannelCreated Tests

        [Test]
        public async Task DiscordSocketClientChannelCreated_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockChannel = new Mock<ISocketChannel>();

            fakeDiscordSocketClient.FakeChannelCreatedEvent.InvokeAsync(mockChannel.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<ChannelCreatedNotification>(y =>
                ReferenceEquals(y.Channel, mockChannel.Object))));
        }

        #endregion DiscordSocketClient.ChannelCreated Tests

        #region DiscordSocketClient.ChannelUpdated Tests

        [Test]
        public async Task DiscordSocketClientChannelUpdated_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockOldChannel = new Mock<ISocketChannel>();
            var mockNewChannel = new Mock<ISocketChannel>();

            fakeDiscordSocketClient.FakeChannelUpdatedEvent.InvokeAsync(mockOldChannel.Object, mockNewChannel.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<ChannelUpdatedNotification>(y =>
                ReferenceEquals(y.OldChannel, mockOldChannel.Object)
                && ReferenceEquals(y.NewChannel, mockNewChannel.Object))));
        }

        #endregion DiscordSocketClient.ChannelUpdated Tests

        #region DiscordSocketClient.GuildAvailable Tests

        [Test]
        public async Task DiscordSocketClientGuildAvailable_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockGuild = new Mock<ISocketGuild>();

            fakeDiscordSocketClient.FakeGuildAvailableEvent.InvokeAsync(mockGuild.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<GuildAvailableNotification>(y =>
                ReferenceEquals(y.Guild, mockGuild.Object))));
        }

        #endregion DiscordSocketClient.GuildAvailable Tests

        #region DiscordSocketClient.JoinedGuild Tests

        [Test]
        public async Task DiscordSocketClientJoinedGuild_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockGuild = new Mock<ISocketGuild>();

            fakeDiscordSocketClient.FakeJoinedGuildEvent.InvokeAsync(mockGuild.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<JoinedGuildNotification>(y =>
                ReferenceEquals(y.Guild, mockGuild.Object))));
        }

        #endregion DiscordSocketClient.JoinedGuild Tests

        #region DiscordSocketClient.MessageDeleted Tests

        [Test]
        public async Task DiscordSocketClientMessageDeleted_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockMessage = new Mock<ICacheable<IMessage, ulong>>();
            var mockChannel = new Mock<IISocketMessageChannel>();

            fakeDiscordSocketClient.FakeMessageDeletedEvent.InvokeAsync(mockMessage.Object, mockChannel.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<MessageDeletedNotification>(y =>
                ReferenceEquals(y.Message, mockMessage.Object)
                && ReferenceEquals(y.Channel, mockChannel.Object))));
        }

        #endregion DiscordSocketClient.MessageDeleted Tests

        #region DiscordSocketClient.MessageReceived Tests

        [Test]
        public async Task DiscordSocketClientMessageReceived_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockMessage = new Mock<ISocketMessage>();

            fakeDiscordSocketClient.FakeMessageReceivedEvent.InvokeAsync(mockMessage.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<MessageReceivedNotification>(y =>
                ReferenceEquals(y.Message, mockMessage.Object))));
        }

        #endregion DiscordSocketClient.MessageReceived Tests

        #region DiscordSocketClient.MessageUpdated Tests

        [Test]
        public async Task DiscordSocketClientMessageUpdated_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockOldMessage = new Mock<ICacheable<IMessage, ulong>>();
            var mockNewMessage = new Mock<ISocketMessage>();
            var mockChannel = new Mock<IISocketMessageChannel>();

            fakeDiscordSocketClient.FakeMessageUpdatedEvent.InvokeAsync(mockOldMessage.Object, mockNewMessage.Object, mockChannel.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<MessageUpdatedNotification>(y =>
                ReferenceEquals(y.OldMessage, mockOldMessage.Object)
                && ReferenceEquals(y.NewMessage, mockNewMessage.Object)
                && ReferenceEquals(y.Channel, mockChannel.Object))));
        }

        #endregion DiscordSocketClient.MessageUpdated Tests

        #region DiscordSocketClient.ReactionAdded Tests

        [Test]
        public async Task DiscordSocketClientReactionAdded_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockMessage = new Mock<ICacheable<IUserMessage, ulong>>();
            var mockChannel = new Mock<IISocketMessageChannel>();
            var mockReaction = new Mock<ISocketReaction>();

            fakeDiscordSocketClient.FakeReactionAddedEvent.InvokeAsync(mockMessage.Object, mockChannel.Object, mockReaction.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<ReactionAddedNotification>(y =>
                ReferenceEquals(y.Message, mockMessage.Object)
                && ReferenceEquals(y.Channel, mockChannel.Object)
                && ReferenceEquals(y.Reaction, mockReaction.Object))));
        }

        #endregion DiscordSocketClient.ReactionAdded Tests

        #region DiscordSocketClient.ReactionRemoved Tests

        [Test]
        public async Task DiscordSocketClientReactionRemoved_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockMessage = new Mock<ICacheable<IUserMessage, ulong>>();
            var mockChannel = new Mock<IISocketMessageChannel>();
            var mockReaction = new Mock<ISocketReaction>();

            fakeDiscordSocketClient.FakeReactionRemovedEvent.InvokeAsync(mockMessage.Object, mockChannel.Object, mockReaction.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<ReactionRemovedNotification>(y =>
                ReferenceEquals(y.Message, mockMessage.Object)
                && ReferenceEquals(y.Channel, mockChannel.Object)
                && ReferenceEquals(y.Reaction, mockReaction.Object))));
        }

        #endregion DiscordSocketClient.ReactionRemoved Tests

        #region DiscordSocketClient.Ready Tests

        [Test]
        public async Task DiscordSocketClientReady_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            fakeDiscordSocketClient.FakeReadyEvent.InvokeAsync()
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(ReadyNotification.Default));
        }

        #endregion DiscordSocketClient.Ready Tests

        #region DiscordSocketClient.UserBanned Tests

        [Test]
        public async Task DiscordSocketClientUserBanned_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockUser = new Mock<ISocketUser>();
            var mockGuild = new Mock<ISocketGuild>();

            fakeDiscordSocketClient.FakeUserBannedEvent.InvokeAsync(mockUser.Object, mockGuild.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<UserBannedNotification>(y =>
                ReferenceEquals(y.User, mockUser.Object)
                && ReferenceEquals(y.Guild, mockGuild.Object))));
        }

        #endregion DiscordSocketClient.UserBanned Tests

        #region DiscordSocketClient.UserJoined Tests

        [Test]
        public async Task DiscordSocketClientUserJoined_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockGuildUser = new Mock<ISocketGuildUser>();

            fakeDiscordSocketClient.FakeUserJoinedEvent.InvokeAsync(mockGuildUser.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<UserJoinedNotification>(y =>
                ReferenceEquals(y.GuildUser, mockGuildUser.Object))));
        }

        #endregion DiscordSocketClient.UserJoined Tests

        #region DiscordSocketClient.UserLeft Tests
        
        [Test]
        public async Task DiscordSocketClientUserLeft_Always_DispatchesNotification()
        {
            var autoMocker = new AutoMocker();
            var fakeDiscordSocketClient = new FakeDiscordSocketClient();
            autoMocker.Use<IDiscordSocketClient>(fakeDiscordSocketClient);
            var mockMessageDispatcher = autoMocker.GetMock<IMessageDispatcher>();

            var uut = autoMocker.CreateInstance<DiscordSocketListeningBehavior>();

            await uut.StartAsync();

            var mockGuildUser = new Mock<ISocketGuildUser>();

            fakeDiscordSocketClient.FakeUserLeftEvent.InvokeAsync(mockGuildUser.Object)
                .IsCompleted.ShouldBeTrue();

            mockMessageDispatcher.ShouldHaveReceived(x => x.Dispatch(It.Is<UserLeftNotification>(y =>
                ReferenceEquals(y.GuildUser, mockGuildUser.Object))));
        }

        #endregion DiscordSocketClient.UserLeft Tests
    }
}
