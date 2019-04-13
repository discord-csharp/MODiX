using System.Threading.Tasks;

using NUnit.Framework;
using Shouldly;

using Discord;

using Modix.Services.Core;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class ReadySynchronizationProviderTests
    {
        #region WhenReady Tests

        [Test]
        public void WhenReady_ReadyNotificationHasNotOccurred_IsNotCompleted()
        {
            var uut = new ReadySynchronizationProvider();

            uut.WhenReady.IsCompleted.ShouldBeFalse();
        }

        [Test]
        public async Task WhenReady_ReadyNotificationHasOccurred_IsCompleted()
        {
            var uut = new ReadySynchronizationProvider();

            await uut.HandleNotificationAsync(ReadyNotification.Default);

            var result = uut.WhenReady;

            await result;

            result.IsCompleted.ShouldBeTrue();
        }

        #endregion WhenReady Tests

        #region HandleNotificationAsync() Tests

        [Test]
        public void HandleNotificationAsync_Always_CompletesImmediately()
        {
            var uut = new ReadySynchronizationProvider();

            uut.HandleNotificationAsync(ReadyNotification.Default)
                .IsCompleted.ShouldBeTrue();
        }

        [Test]
        public async Task HandleNotificationAsync_HasBeenInvoked_DoesNotThrowException()
        {
            var uut = new ReadySynchronizationProvider();

            await uut.HandleNotificationAsync(ReadyNotification.Default);

            await uut.HandleNotificationAsync(ReadyNotification.Default);
        }

        #endregion HandleNotificationAsync() Tests
    }
}
