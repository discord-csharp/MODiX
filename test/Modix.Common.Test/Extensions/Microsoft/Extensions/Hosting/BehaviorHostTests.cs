using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Moq;
using NUnit.Framework;
using Shouldly;

namespace Modix.Common.Test.Extensions.Microsoft.Extensions.Hosting
{
    [TestFixture]
    public class BehaviorHostTests
    {
        #region Test Context

        internal class MockBehavior
            : Mock<IBehavior>
        {
            public MockBehavior()
            {
                Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
                    .Returns(_startCompletionSource.Task);

                Setup(x => x.StopAsync(It.IsAny<CancellationToken>()))
                    .Returns(_stopCompletionSource.Task);
            }

            public void CompleteStart()
                => _startCompletionSource.SetResult(null);

            public void CompleteStop()
                => _stopCompletionSource.SetResult(null);

            private readonly TaskCompletionSource<object?> _startCompletionSource
                = new TaskCompletionSource<object?>();
            private readonly TaskCompletionSource<object?> _stopCompletionSource
                = new TaskCompletionSource<object?>();
        }

        internal class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public readonly List<MockBehavior> MockBehaviors
                = new List<MockBehavior>();

            public BehaviorHost BuildUut()
                => new BehaviorHost(
                    MockBehaviors.Select(x => x.Object),
                    LoggerFactory.CreateLogger<BehaviorHost>());
        }

        #endregion Test Context

        #region StartAsync() Tests

        [Test]
        public void StartAsync_BehaviorsIsEmpty_CompletesImmediately()
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            var result = uut.StartAsync(
                testContext.CancellationToken);

            result.IsCompletedSuccessfully.ShouldBeTrue();
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task StartAsync_Always_StartsAllBehaviorsInParallel(
            int behaviorCount)
        {
            using var testContext = new TestContext();

            Enumerable.Range(0, behaviorCount)
                .Select(_ => new MockBehavior())
                .ForEach(x => testContext.MockBehaviors.Add(x));

            var uut = testContext.BuildUut();

            var result = uut.StartAsync(
                testContext.CancellationToken);

            result.IsCompleted.ShouldBeFalse();

            testContext.MockBehaviors
                .ForEach(mockBehavior => mockBehavior.ShouldHaveReceived(x => x
                    .StartAsync(testContext.CancellationToken)));

            testContext.MockBehaviors
                .ForEach(mockBehavior => mockBehavior.CompleteStart());

            await result;
        }

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public void StopAsync_BehaviorsIsEmpty_CompletesImmediately()
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            var result = uut.StopAsync(
                testContext.CancellationToken);

            result.IsCompletedSuccessfully.ShouldBeTrue();
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task StopAsync_Always_StopsAllBehaviorsInParallel(
            int behaviorCount)
        {
            using var testContext = new TestContext();

            Enumerable.Range(0, behaviorCount)
                .Select(_ => new MockBehavior())
                .ForEach(x => testContext.MockBehaviors.Add(x));

            var uut = testContext.BuildUut();

            var result = uut.StopAsync(
                testContext.CancellationToken);

            result.IsCompleted.ShouldBeFalse();

            testContext.MockBehaviors
                .ForEach(mockBehavior => mockBehavior.ShouldHaveReceived(x => x
                    .StopAsync(testContext.CancellationToken)));

            testContext.MockBehaviors
                .ForEach(mockBehavior => mockBehavior.CompleteStop());

            await result;
        }

        #endregion StopAsync() Tests
    }
}
