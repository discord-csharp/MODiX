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
    public class AuthorizationAutoConfigBehaviorTests
    {
        #region HandleNotificationAsync(GuildAvailableNotification) Tests

        [Test]
        public async Task HandleNotificationAsync_GuildAvailableNotification_Always_InvokesAutoConfigureGuildAsync()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<AuthorizationAutoConfigBehavior>();

            var guild = autoMocker.Get<ISocketGuild>();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var notification = new GuildAvailableNotification(guild);

                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                autoMocker.GetMock<IAuthorizationService>()
                    .ShouldHaveReceived(x => x.AutoConfigureGuildAsync(guild, cancellationTokenSource.Token));
            }
        }

        #endregion HandleNotificationAsync(GuildAvailableNotification) Tests

        #region HandleNotificationAsync(JoinedGuildNotification) Tests

        [Test]
        public void HandleNotificationAsync_JoinedGuildNotification_GuildIsNotAvailable_CompletesImmediately()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<AuthorizationAutoConfigBehavior>();

            var mockGuild = autoMocker.GetMock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Available)
                .Returns(false);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var notification = new JoinedGuildNotification(mockGuild.Object);

                uut.HandleNotificationAsync(notification, cancellationTokenSource.Token)
                    .IsCompleted.ShouldBeTrue();

                autoMocker.GetMock<IAuthorizationService>()
                    .ShouldNotHaveReceived(x => x.AutoConfigureGuildAsync(It.IsAny<IGuild>(), It.IsAny<CancellationToken>()));
            }
        }

        [Test]
        public async Task HandleNotificationAsync_JoinedGuildNotification_GuildIsAvailable_InvokesAutoConfigureGuildAsync()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<AuthorizationAutoConfigBehavior>();

            var mockGuild = autoMocker.GetMock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Available)
                .Returns(true);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var notification = new JoinedGuildNotification(mockGuild.Object);

                await uut.HandleNotificationAsync(notification, cancellationTokenSource.Token);

                autoMocker.GetMock<IAuthorizationService>()
                    .ShouldHaveReceived(x => x.AutoConfigureGuildAsync(mockGuild.Object, cancellationTokenSource.Token));
            }
        }

        #endregion HandleNotificationAsync(JoinedGuildNotification) Tests
    }
}
