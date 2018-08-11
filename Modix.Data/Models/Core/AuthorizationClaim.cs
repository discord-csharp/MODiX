namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the types of claims that can be used to authorize a request.
    /// </summary>
    public enum AuthorizationClaim
    {
        /// <summary>
        /// Authorizes a request to configure the authorization feature.
        /// </summary>
        AuthorizationConfigure,
        /// <summary>
        /// Authorizes a request to read infraction/moderation data.
        /// </summary>
        ModerationRead,
        /// <summary>
        /// Authorizes a request to attach a note upon a user.
        /// </summary>
        ModerationNote,
        /// <summary>
        /// Authorizes a request to issue a warning to a user.
        /// </summary>
        ModerationWarn,
        /// <summary>
        /// Authorizes a request to mute a user.
        /// </summary>
        ModerationMute,
        /// <summary>
        /// Authorizes a request to ban a user.
        /// </summary>
        ModerationBan,
        /// <summary>
        /// Authorizes a request to configure the moderation feature.
        /// </summary>
        ModerationConfigure,
        /// <summary>
        /// Authorizes a request to rescind an infraction upon a user.
        /// </summary>
        ModerationRescind,
        /// <summary>
        /// Authorizes a request to delete an infraction upon a user.
        /// </summary>
        ModerationDeleteInfraction,
        /// <summary>
        /// Authorizes a request to delete a message from a guild.
        /// </summary>
        ModerationDeleteMessage,
        /// <summary>
        /// Authorizes a request to post a message to a guild, containing a Discord invite link.
        /// </summary>
        PostInviteLink
        /// <summary>
        /// Authorizes a request to create a promotion campaign for a user.
        /// </summary>
        PromotionCreate,
        /// <summary>
        /// Authorizes a request to comment on a promotion campaign for a user.
        /// </summary>
        PromotionComment,
        /// <summary>
        /// Authorizes a request to approve/deny/reactivate a promotion campaign for a user.
        /// </summary>
        PromotionExecute
    }
}
