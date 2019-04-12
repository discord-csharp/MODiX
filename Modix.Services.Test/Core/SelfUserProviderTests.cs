using System;
using System.Threading;
using System.Threading.Tasks;

using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

using Discord.WebSocket;

using Modix.Services.Core;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class SelfUserProviderTests
    {
        #region GetSelfUserAsnyc() Tests

        [Test]
        public async Task GetSelfUserAsync_WhenReadyIsNotComplete_WaitsForWhenReady()
        {
            var autoMocker = new AutoMocker();

            var whenReadySource = new TaskCompletionSource<object>();
            autoMocker.GetMock<IReadySynchronizationProvider>()
                .Setup(x => x.WhenReady)
                .Returns(() => whenReadySource.Task);

            var selfUser = autoMocker.Get<ISocketSelfUser>();
            autoMocker.GetMock<IDiscordSocketClient>()
                .Setup(x => x.CurrentUser)
                .Returns(() => autoMocker.Get<ISocketSelfUser>());

            var uut = autoMocker.CreateInstance<SelfUserProvider>();

            var result = uut.GetSelfUserAsync();
            result.IsCompleted.ShouldBeFalse();

            whenReadySource.SetResult(null);
            await result;

            result.IsCompleted.ShouldBeTrue();
            result.Result.ShouldBeSameAs(selfUser);
        }

        [Test]
        public void GetSelfUserAsync_WhenReadyIsComplete_CompletesImmediately()
        {
            var autoMocker = new AutoMocker();

            autoMocker.GetMock<IReadySynchronizationProvider>()
                .Setup(x => x.WhenReady)
                .Returns(() => Task.CompletedTask);

            var selfUser = autoMocker.Get<ISocketSelfUser>();
            autoMocker.GetMock<IDiscordSocketClient>()
                .Setup(x => x.CurrentUser)
                .Returns(() => autoMocker.Get<ISocketSelfUser>());

            var uut = autoMocker.CreateInstance<SelfUserProvider>();

            var result = uut.GetSelfUserAsync();
            result.IsCompleted.ShouldBeTrue();

            result.Result.ShouldBeSameAs(selfUser);
        }

        [Test]
        public void GetSelfUserAsync_CancellationTokenIsCancelled_ThrowsCancellation()
        {
            var autoMocker = new AutoMocker();

            autoMocker.GetMock<IReadySynchronizationProvider>()
                .Setup(x => x.WhenReady)
                .Returns(() => Task.Delay(-1));

            var uut = autoMocker.CreateInstance<SelfUserProvider>();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var result = uut.GetSelfUserAsync(cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Should.Throw<TaskCanceledException>(result);
            }
        }

        #endregion GetSelfUserAsnyc() Tests
    }
}
