using NUnit.Framework;

namespace Modix.Bot.Test.Behaviors
{
    [TestFixture]
    public class PromotionDialogStartupBehaviorTests
    {
        #region StartAsync() Tests

        [Test]
        public void StartAsync_Always_DeletesDialogsForClosedCampaignsAndCachesDialogsForOpenCampaigns()
            => throw new IgnoreException("Not yet implemented");
            // PromotionDialogRepository.ShouldHaveReceived(x => x.GetDialogsAsync())
            // ServiceScope.ShouldHaveReceived(x => x.Dispose())
            // For Each Closed Campaign
            //      ProotionDialogRepository.ShouldHaveReceived(x => x.TryDeleteAsync(...))
            // For Each Open Campaign
            //      MemoryCache.ShouldHaveReceived(x => x.Set(...))

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public void StopAsync_Always_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // uut.StopAsync().IsCompletedSuccessfully.ShouldBeTrue()

        #endregion StopAsync() Tests
    }
}
