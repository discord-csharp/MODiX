namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the possible types of moderation actions that can be performed.
    /// </summary>
    public enum ConfigurationActionType
    {
        /// <summary>
        /// Describes an action where a claim mapping was created.
        /// </summary>
        ClaimMappingCreated,
        /// <summary>
        /// Describes an action where a claim mapping was rescinded.
        /// </summary>
        ClaimMappingRescinded,
    }
}
