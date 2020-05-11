using NUnit.Framework;

namespace Modix.Bot.Test.Behaviors
{
    [TestFixture]
    public class PromotionDialogBehaviorTests
    {
        #region HandleNotificationAsync(PromotionActionCreatedNotification) Tests

        [Test]
        public void HandleNotificationAsync_PromotionActionCreatedNotification_ActionTypeIsCampaignCreated_NoPromotionInterfaceIsDesignated_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // DesignatedChannelService.ShouldHaveReceived(x => x.GetDesignatedChannelsAsync(...))
            // PromotionDialogRepository.Invocations.ShouldBeEmpty()
            // MemoryCache.Invocations.ShouldBeEmpty()

        [Test]
        public void HandleNotificationAsync_PromotionActionCreatedNotification_ActionTypeIsCampaignCreated_AnyPromotionInterfaceIsDesignated_CreatesAndInitializesDialog()
            => throw new IgnoreException("Not yet implemented");
            // DesignatedChannelService.ShouldHaveReceived(x => x.GetDesignatedChannelsAsync(...))
            // For Each DesignatedChannel
            //      PromotionDialogRepository.ShouldHaveReceived(x => x.CreateAsync(...))
            //      Channel.ShouldHaveReceived(x => x.SendMessageAsync(...))
            //      Message.ShouldHaveReceived(x => x.AddReactionAsync(...))
            // MemoryCache.ShouldHaveReceived(x => x.Set(...))

        [Test]
        public void HandleNotificationAsync_PromotionActionCreatedNotification_ActionTypeIsCampaignClosed_CacheDoesNotContainDialogs_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // PromotionDialogRepository.Invocations.ShouldBeEmpty()
            // DesignatedChannelService.Invocations.ShouldBeEmpty()

        [Test]
        public void HandleNotificationAsync_PromotionActionCreatedNotification_ActionTypeIsCampaignClosed_CacheContainsAnyDialogs_DeletesDialogs()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.ShouldHaveReceived(x => x.Get(...))
            // For Each CachedDialog
            //      PromotionDialogRepository.ShouldHaveReceived(x => x.DeleteAsync())
            //      MessageService.ShouldHaveReceived(x => x.FindMessageAsync())
            //      Message.ShouldHaveReceived(x => x.DeleteAsync())
            // MemoryCache.ShouldHaveReceived(x => x.Remove(...))

        #endregion HandleNotificationAsync(PromotionActionCreatedNotification) Tests

        #region HandleNotificationAsync(ReactionAddedNotification) Tests

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_ReactionUserIsBot_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.Invocations.ShouldBeEmpty()
            // Notification.Message.Invocations.ShouldBeEmpty()
            // PromotionService.Invocations.ShouldBeEmpty()
            // PromotionCampaignRepository.Invocations.ShouldBeEmpty()

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_ReactionEmoteIsIrrelevant_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.Invocations.ShouldBeEmpty()
            // Notification.Message.Invocations.ShouldBeEmpty()
            // AuthenticationService.Invocations.ShouldBeEmpty()
            // PromotionService.Invocations.ShouldBeEmpty()
            // PromotionCampaignRepository.Invocations.ShouldBeEmpty()

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_MemoryCacheContainsNoDialogs_DoesNothing()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.ShouldHaveReceived(x => x.TryGetValue(...))
            // Notification.Message.Invocations.ShouldBeEmpty()
            // AuthenticationService.Invocations.ShouldBeEmpty()
            // PromotionService.Invocations.ShouldBeEmpty()
            // PromotionCampaignRepository.Invocations.ShouldBeEmpty()

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_AuthenticationThrowsException_RendersExceptionMessage()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.ShouldHaveReceived(x => x.TryGetValue(...))
            // Message.ShouldHaveReceived(x => x.RemoveReactionAsync(...))
            // AuthenticationService.ShouldHaveReceived(x => x.OnAuthenticatedAsync(...))
            // PromotionService.Invocations.ShouldBeEmpty()
            // PromotionCampaignRepository.ShouldHaveReceived(x => x.GetCampaignSummaryByIdAsync(...))
            // Message.ShouldHaveReceived(x => x.ModifyAsync(...))

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_UpdateOrAddCommentThrowsException_RendersExceptionMessage()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.ShouldHaveReceived(x => x.TryGetValue(...))
            // Message.ShouldHaveReceived(x => x.RemoveReactionAsync(...))
            // AuthenticationService.ShouldHaveReceived(x => x.OnAuthenticatedAsync(...))
            // PromotionService.ShouldHaveReceived(x => x.UpdateOrAddCommentAsync(...))
            // PromotionCampaignRepository.ShouldHaveReceived(x => x.GetCampaignSummaryByIdAsync(...))
            // Message.ShouldHaveReceived(x => x.ModifyAsync(...))

        [Test]
        public void HandleNotificationAsync_ReactionAddedNotification_Otherwise_PerformsUpdateOrAddCommentAndRendersCampaign()
            => throw new IgnoreException("Not yet implemented");
            // MemoryCache.ShouldHaveReceived(x => x.TryGetValue(...))
            // Message.ShouldHaveReceived(x => x.RemoveReactionAsync(...))
            // AuthenticationService.ShouldHaveReceived(x => x.OnAuthenticatedAsync(...))
            // PromotionService.ShouldHaveReceived(x => x.UpdateOrAddCommentAsync(...))
            // PromotionCampaignRepository.ShouldHaveReceived(x => x.GetCampaignSummaryByIdAsync(...))
            // Message.ShouldHaveReceived(x => x.ModifyAsync(...))

        #endregion HandleNotificationAsync(ReactionAddedNotification) Tests
    }
}
