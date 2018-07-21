namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Defines the possible types of claim mappings.
    /// </summary>
    public enum ClaimMappingType
    {
        /// <summary>
        /// Describes a claim mapping where a claim is granted to an entity.
        /// </summary>
        Granted,
        /// <summary>
        /// Describes a claim mapping where a claim is denied to an entity.
        /// </summary>
        Denied,
    }
}
