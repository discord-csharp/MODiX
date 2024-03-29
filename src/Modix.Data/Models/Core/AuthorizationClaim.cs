using Modix.Data.Utilities;
using static Modix.Data.Models.Core.AuthorizationClaimCategory;

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
        [ClaimInfo(Configuration, "Authorizes a request to configure the authorization feature.")]
        AuthorizationConfigure,
        /// <summary>
        /// Authorizes a request to read infraction/moderation data.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to read infraction/moderation data.")]
        ModerationRead,
        /// <summary>
        /// Authorizes a request to attach a note upon a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to attach a note upon a user.")]
        ModerationNote,
        /// <summary>
        /// Authorizes a request to issue a warning to a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to issue a warning to a user.")]
        ModerationWarn,
        /// <summary>
        /// Authorizes a request to mute a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to mute a user.")]
        ModerationMute,
        /// <summary>
        /// Authorizes a request to ban a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to ban a user.")]
        ModerationBan,
        /// <summary>
        /// Authorizes a request to configure the moderation feature.
        /// </summary>
        [ClaimInfo(Configuration, "Authorizes a request to configure the moderation feature.")]
        ModerationConfigure,
        /// <summary>
        /// Authorizes a request to rescind an infraction upon a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to rescind an infraction upon a user.")]
        ModerationRescind,
        /// <summary>
        /// Authorizes a request to delete an infraction upon a user.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to delete an infraction upon a user.")]
        ModerationDeleteInfraction,
        /// <summary>
        /// Authorizes a request to update an infraction.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to update an infraction.")]
        ModerationUpdateInfraction,
        /// <summary>
        /// Authorizes a request to delete a message from a guild.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to delete a message from a guild.")]
        ModerationDeleteMessage,
        /// <summary>
        /// Authorizes a request to mass-delete messages from a guild.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to mass-delete messages from a guild.")]
        ModerationMassDeleteMessages,
        /// <summary>
        /// Authorizes a request to view deleted message logs.
        /// </summary>
        [ClaimInfo(Log, "Authorizes a request to view deleted message logs.")]
        LogViewDeletedMessages,
        /// <summary>
        /// Authorizes a request to post a message to a guild, containing content that is in a pattern check.
        /// </summary>
        [ClaimInfo(Misc, "Authorizes a request to post a message to a guild, containing blocked content.")]
        BypassMessageContentPatternCheck,
        /// <summary>
        /// Authorizes a request to create a designated channel mapping.
        /// </summary>
        [ClaimInfo(DesignatedChannels, "Authorizes a request to create a designated channel mapping.")]
        DesignatedChannelMappingCreate,
        /// <summary>
        /// Authorizes a request to read designated channel mappings.
        /// </summary>
        [ClaimInfo(DesignatedChannels, "Authorizes a request to read designated channel mappings.")]
        DesignatedChannelMappingRead,
        /// <summary>
        /// Authorizes a request to delete a designated channel mapping.
        /// </summary>
        [ClaimInfo(DesignatedChannels, "Authorizes a request to delete a designated channel mapping.")]
        DesignatedChannelMappingDelete,
        /// <summary>
        /// Authorizes a request to create a designated role mapping.
        /// </summary>
        [ClaimInfo(DesignatedRoles, "Authorizes a request to create a designated role mapping.")]
        DesignatedRoleMappingCreate,
        /// <summary>
        /// Authorizes a request to read designated role mappings.
        /// </summary>
        [ClaimInfo(DesignatedRoles, "Authorizes a request to read designated role mappings.")]
        DesignatedRoleMappingRead,
        /// <summary>
        /// Authorizes a request to delete a designated role mapping.
        /// </summary>
        [ClaimInfo(DesignatedRoles, "Authorizes a request to delete a designated role mapping.")]
        DesignatedRoleMappingDelete,
        /// <summary>
        /// Authorizes a request to create a promotion campaign for a user.
        /// </summary>
        [ClaimInfo(PromotionActions, "Authorizes a request to create a promotion campaign for a user.")]
        PromotionsCreateCampaign,
        /// <summary>
        /// Authorizes a request to close a promotion campaign for a user.
        /// </summary>
        [ClaimInfo(PromotionActions, "Authorizes a request to close a promotion campaign for a user.")]
        PromotionsCloseCampaign,
        /// <summary>
        /// Authorizes a request to comment on a promotion campaign for a user.
        /// </summary>
        [ClaimInfo(PromotionActions, "Authorizes a request to comment on a promotion campaign for a user.")]
        PromotionsComment,
        /// <summary>
        /// Authorizes a request to read promotion campaign data.
        /// </summary>
        [ClaimInfo(PromotionActions, "Authorizes a request to read promotion campaign data.")]
        PromotionsRead,
        /// <summary>
        /// Authorizes a request to perform a count for a popularity contest
        /// </summary>
        [ClaimInfo(Misc, "Authorizes a request to perform a count for a popularity contest")]
        PopularityContestCount,
        /// <summary>
        /// Authorizes a request to mention a role that has restricted mentionability.
        /// </summary>
        [ClaimInfo(Misc, "Authorizes a request to mention a role that has restricted mentionability.")]
        MentionRestrictedRole,
        /// <summary>
        /// Authorizes a request to create a tag.
        /// </summary>
        [ClaimInfo(TagActions, "Authorizes a request to create a tag.")]
        CreateTag,
        /// <summary>
        /// Authorizes a request to invoke a tag.
        /// </summary>
        [ClaimInfo(TagActions, "Authorizes a request to invoke a tag.")]
        UseTag,
        /// <summary>
        /// Authorizes a request to maintain a tag that was not created by the requesting user.
        /// </summary>
        [ClaimInfo(TagActions, "Authorizes a request to maintain a tag that was not created by the requesting user.")]
        MaintainOtherUserTag,
        /// <summary>
        /// Authorizes a request to create a giveaway and determine its winners.
        /// </summary>
        [ClaimInfo(Misc, "Authorizes a request to create a giveaway and determine its winners.")]
        ExecuteGiveaway,
        /// <summary>
        /// Authorizes a request to manage message patterns.
        /// </summary>
        [ClaimInfo(ModerationActions, "Authorizes a request to manage message patterns.")]
        ManageMessageContentPatterns,
    }
}
