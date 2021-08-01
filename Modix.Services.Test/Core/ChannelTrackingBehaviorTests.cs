using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

using Modix.Services.Core;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class ChannelTrackingBehaviorTests
    {
        #region Test Cases

        private static readonly Type[] ChannelIsNotGuildChannelTestCases
            = {
                typeof(ISocketChannel),
                typeof(ISocketDMChannel),
                typeof(ISocketGroupChannel)
            };

        private static readonly Type[] GuildChannelIsNotTextChannelTestCases
            = {
                typeof(ISocketCategoryChannel),
                typeof(ISocketGuildChannel),
                typeof(ISocketVoiceChannel),
            };

        private static readonly Type[][] GuildChannelsNoneAreTextChannelsTestCases
            = GuildChannelIsNotTextChannelTestCases
                .Select(x => new[] { x })
                .Concat(GuildChannelIsNotTextChannelTestCases
                    .SelectMany(x => GuildChannelIsNotTextChannelTestCases
                        .Select(y => new[] { x, y })))
                .Append(GuildChannelIsNotTextChannelTestCases)
                .ToArray();

        private static readonly Type[][] GuildChannelsAnyAreTextChannelsTestCases
            = Enumerable.Empty<Type[]>()
                .Append(new[] { typeof(ISocketTextChannel) })
                .Append(new[] { typeof(ISocketTextChannel), typeof(ISocketTextChannel) })
                .Concat(GuildChannelIsNotTextChannelTestCases
                    .Select(x => new[] { x, typeof(ISocketTextChannel) }))
                .Append(GuildChannelIsNotTextChannelTestCases
                    .Prepend(typeof(ISocketTextChannel))
                    .Append(typeof(ISocketTextChannel))
                    .ToArray())
                .ToArray();

        #endregion Test Cases

        #region HandleNotificationAsync(ChannelCreatedNotification)

        [TestCaseSource(nameof(ChannelIsNotGuildChannelTestCases))]
        [TestCaseSource(nameof(GuildChannelIsNotTextChannelTestCases))]
        public void HandleNotificationAsync_ChannelCreatedNotification_ChannelIsNotTextChannel_CompletesImmediately(Type channelType)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannel = typeof(Mock<>).MakeGenericType(channelType).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock;

            var notification = new ChannelCreatedNotification(mockChannel.Object as ISocketChannel);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompletedSuccessfully.ShouldBeTrue();

                autoMocker.GetMock<IChannelService>()
                    .Invocations.ShouldBeEmpty();
            }
        }

        [Test]
        public async Task HandleNotificationAsync_ChannelCreatedNotification_ChannelIsTextChannel_TracksChannel()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannel = new Mock<ISocketTextChannel>();

            var notification = new ChannelCreatedNotification(mockChannel.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                autoMocker.GetMock<IChannelService>()
                    .ShouldHaveReceived(x => x.TrackChannelAsync(mockChannel.Object.Name, mockChannel.Object.Id, mockChannel.Object.GuildId, cancellationTokenSource.Token));
            }
        }

        #endregion HandleNotificationAsync(ChannelCreatedNotification)

        #region HandleNotificationAsync(ChannelUpdatedNotification)

        [TestCaseSource(nameof(ChannelIsNotGuildChannelTestCases))]
        [TestCaseSource(nameof(GuildChannelIsNotTextChannelTestCases))]
        public void HandleNotificationAsync_ChannelUpdatedNotification_ChannelIsNotTextChannel_CompletesImmediately(Type channelType)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockOldChannel = typeof(Mock<>).MakeGenericType(channelType).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock;
            var mockNewChannel = typeof(Mock<>).MakeGenericType(channelType).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock;

            var notification = new ChannelUpdatedNotification(mockOldChannel.Object as ISocketChannel, mockNewChannel.Object as ISocketChannel);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompletedSuccessfully.ShouldBeTrue();

                autoMocker.GetMock<IChannelService>()
                    .Invocations.ShouldBeEmpty();
            }
        }

        [Test]
        public async Task HandleNotificationAsync_ChannelUpdatedNotification_ChannelIsTextChannel_TracksChannel()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockOldChannel = new Mock<ISocketTextChannel>();
            var mockNewChannel = new Mock<ISocketTextChannel>();

            var notification = new ChannelUpdatedNotification(mockOldChannel.Object, mockNewChannel.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                autoMocker.GetMock<IChannelService>()
                    .ShouldHaveReceived(x => x.TrackChannelAsync(mockNewChannel.Object.Name, mockNewChannel.Object.Id, mockNewChannel.Object.GuildId, cancellationTokenSource.Token));
            }
        }

        #endregion HandleNotificationAsync(ChannelUpdatedNotification)

        #region HandleNotificationAsync(GuildAvailableNotification)

        [TestCaseSource(nameof(GuildChannelsNoneAreTextChannelsTestCases))]
        public void HandleNotificationAsync_GuildAvailableNotification_NoChannelsAreTextChannel_CompletesImmediately(params Type[] channelTypes)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannels = channelTypes
                .Select(x => typeof(Mock<>).MakeGenericType(x).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock)
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Channels)
                .Returns(mockChannels
                    .Select(x => x.Object as ISocketGuildChannel)
                    .ToArray());

            var notification = new GuildAvailableNotification(mockGuild.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompletedSuccessfully.ShouldBeTrue();

                autoMocker.GetMock<IChannelService>()
                    .Invocations.ShouldBeEmpty();
            }
        }

        [TestCaseSource(nameof(GuildChannelsAnyAreTextChannelsTestCases))]
        public async Task HandleNotificationAsync_GuildAvailableNotification_AnyChannelIsTextChannel_TracksTextChannels(params Type[] channelTypes)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannels = channelTypes
                .Select(x => typeof(Mock<>).MakeGenericType(x).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock)
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Channels)
                .Returns(mockChannels
                    .Select(x => x.Object as ISocketGuildChannel)
                    .ToArray());

            var notification = new GuildAvailableNotification(mockGuild.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                var mockChannelService = autoMocker.GetMock<IChannelService>();

                var textChannels = mockChannels
                    .Select(x => x.Object)
                    .OfType<ISocketTextChannel>()
                    .ToArray();

                foreach (var textChannel in textChannels)
                    mockChannelService.ShouldHaveReceived(x => x.TrackChannelAsync(textChannel.Name, textChannel.Id, textChannel.GuildId, cancellationTokenSource.Token));

                mockChannelService
                    .Invocations
                    .Where(x => x.Method.Name == nameof(IChannelService.TrackChannelAsync))
                    .Select(x => x.Arguments[0])
                    .ShouldBe(textChannels.Select(a => a.Name), ignoreOrder: true);
            }
        }

        #endregion HandleNotificationAsync(GuildAvailableNotification)

        #region HandleNotificationAsync(JoinedGuildNotification)

        [TestCaseSource(nameof(GuildChannelsNoneAreTextChannelsTestCases))]
        [TestCaseSource(nameof(GuildChannelsAnyAreTextChannelsTestCases))]
        public void HandleNotificationAsync_JoinedGuildNotification_GuildIsNotAvailable_CompletesImmediately(params Type[] channelTypes)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannels = channelTypes
                .Select(x => typeof(Mock<>).MakeGenericType(x).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock)
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Channels)
                .Returns(mockChannels
                    .Select(x => x.Object as ISocketGuildChannel)
                    .ToArray());
            mockGuild
                .Setup(x => x.Available)
                .Returns(false);

            var notification = new JoinedGuildNotification(mockGuild.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompletedSuccessfully.ShouldBeTrue();

                autoMocker.GetMock<IChannelService>()
                    .Invocations.ShouldBeEmpty();
            }
        }

        [TestCaseSource(nameof(GuildChannelsNoneAreTextChannelsTestCases))]
        public void HandleNotificationAsync_JoinedGuildNotification_NoChannelsAreTextChannel_CompletesImmediately(params Type[] channelTypes)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannels = channelTypes
                .Select(x => typeof(Mock<>).MakeGenericType(x).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock)
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Channels)
                .Returns(mockChannels
                    .Select(x => x.Object as ISocketGuildChannel)
                    .ToArray());
            mockGuild
                .Setup(x => x.Available)
                .Returns(true);

            var notification = new JoinedGuildNotification(mockGuild.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompletedSuccessfully.ShouldBeTrue();

                autoMocker.GetMock<IChannelService>()
                    .Invocations.ShouldBeEmpty();
            }
        }

        [TestCaseSource(nameof(GuildChannelsAnyAreTextChannelsTestCases))]
        public async Task HandleNotificationAsync_JoinedGuildNotification_AnyChannelIsTextChannel_TracksTextChannels(params Type[] channelTypes)
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<ChannelTrackingBehavior>();

            var mockChannels = channelTypes
                .Select(x => typeof(Mock<>).MakeGenericType(x).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as Mock)
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Channels)
                .Returns(mockChannels
                    .Select(x => x.Object as ISocketGuildChannel)
                    .ToArray());
            mockGuild
                .Setup(x => x.Available)
                .Returns(true);

            var notification = new JoinedGuildNotification(mockGuild.Object);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                var mockChannelService = autoMocker.GetMock<IChannelService>();

                var textChannels = mockChannels
                    .Select(x => x.Object)
                    .OfType<ISocketTextChannel>()
                    .ToArray();

                foreach (var textChannel in textChannels)
                    mockChannelService.ShouldHaveReceived(x => x.TrackChannelAsync(textChannel.Name, textChannel.Id, textChannel.GuildId, cancellationTokenSource.Token));

                mockChannelService
                    .Invocations
                    .Where(x => x.Method.Name == nameof(IChannelService.TrackChannelAsync))
                    .Select(x => x.Arguments[0])
                    .ShouldBe(textChannels.Select(a => a.Name), ignoreOrder: true);
            }
        }

        #endregion HandleNotificationAsync(JoinedGuildNotification)
    }
}
