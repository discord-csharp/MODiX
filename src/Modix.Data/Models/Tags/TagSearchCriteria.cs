using System.Linq;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a set of criteria for searching for tags within a repository.
    /// </summary>
    public class TagSearchCriteria
    {
        /// <summary>
        /// The Discord snowflake ID of the guild in which the tags reside.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A partial tag name to filter on.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the owner user of the tags to filter on.
        /// </summary>
        public ulong? OwnerUserId { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the owner role of the tags to filter on.
        /// </summary>
        public ulong? OwnerRoleId { get; set; }
    }

    public static class TagSearchCriteriaExtensions
    {
        public static IQueryable<TagEntity> FilterTagsBy(this IQueryable<TagEntity> query, TagSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId!.Value,
                    !(criteria.GuildId is null))
                .FilterBy(
                    x => x.Name.Contains(criteria.Name!.ToLower()),
                    !(criteria.Name is null))
                .FilterBy(
                    x => x.OwnerUserId != null && x.OwnerUserId == criteria.OwnerUserId!.Value,
                    !(criteria.OwnerUserId is null))
                .FilterBy(
                    x => x.OwnerRoleId != null && x.OwnerRoleId == criteria.OwnerRoleId!.Value,
                    !(criteria.OwnerRoleId is null));
    }
}
