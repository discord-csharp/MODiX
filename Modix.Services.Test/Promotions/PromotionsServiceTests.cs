using NUnit.Framework;

namespace Modix.Services.Test.Promotions
{
    [TestFixture]
    public class PromotionsServiceTests
    {
        #region UpdateOrAddCommentAsync() Tests

        [Test]
        public void UpdateOrAddCommentAsync_UserIsUnauthenticated_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("authenticated");

        [Test]
        public void UpdateOrAddCommentAsync_UserIsMissingClaims_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain(missingClaims);

        [Test]
        public void UpdateOrAddCommentAsync_CampaignDoesNotExist_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("campaign");
            // exception.Message.ShouldContain(campaignId);

        [Test]
        public void UpdateOrAddCommentAsync_CampaignIsClosed_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("campaign");
            // exception.Message.ShouldContain("closed");

        [Test]
        public void UpdateOrAddCommentAsync_UserIsCampaignSubject_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("campaign");
            // exception.Message.ShouldContain("subject");

        [Test]
        public void UpdateOrAddCommentAsync_UserHasInsufficientRank_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("campaign");
            // exception.Message.ShouldContain("rank");

        [Test]
        public void UpdateOrAddCommentAsync_CommentExistsAndSentimentIsSame_ThrowsException()
            => throw new IgnoreException("Not yet implemented");
            // Should.Throw<InvalidOperationException>()
            // exception.Message.ShouldContain("sentiment");

        [Test]
        public void UpdateOrAddCommentAsync_CommentExistsAndSentimentIsDifferent_UpdatesComment()
            => throw new IgnoreException("Not yet implemented");
            // PromotionCommentRepository.ShouldHaveReceived(x => x.TryUpdateAsync(...))
            // MessageDispatcher.ShouldHaveReceived(x => x.Dispatch(PromotionActionCreatedNotification)

        [Test]
        public void UpdateOrAddCommentAsync_CommentDoesNotExists_CreatesComment()
            => throw new IgnoreException("Not yet implemented");
            // PromotionCommentRepository.ShouldHaveReceived(x => x.CreateAsync(...))
            // MessageDispatcher.ShouldHaveReceived(x => x.Dispatch(PromotionActionCreatedNotification)

        #endregion UpdateOrAddCommentAsync() Tests
    }
}
