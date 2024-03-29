using System.Linq;
using System.Collections.Generic;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class DesignatedChannelMappings
    {
        public static readonly IEnumerable<DesignatedChannelMappingEntity> Entities
            = new[]
            {
                new DesignatedChannelMappingEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    Type = DesignatedChannelType.MessageLog,
                    ChannelId = 1,
                    CreateActionId = 6,
                    DeleteActionId = 7
                },
                new DesignatedChannelMappingEntity()
                {
                    Id = 2,
                    GuildId = 2,
                    Type = DesignatedChannelType.ModerationLog,
                    ChannelId = 3,
                    CreateActionId = 8,
                    DeleteActionId = null
                },
                new DesignatedChannelMappingEntity()
                {
                    Id = 3,
                    GuildId = 1,
                    Type = DesignatedChannelType.ModerationLog,
                    ChannelId = 2,
                    CreateActionId = 9,
                    DeleteActionId = null
                },
                new DesignatedChannelMappingEntity()
                {
                    Id = 4,
                    GuildId = 2,
                    Type = DesignatedChannelType.PromotionLog,
                    ChannelId = 3,
                    CreateActionId = 10,
                    DeleteActionId = null
                }
            };

        public static readonly IEnumerable<DesignatedChannelMappingCreationData> Creations
            = new[]
            {
                new DesignatedChannelMappingCreationData()
                {
                    ChannelId = 2,
                    GuildId = 1,
                    Type = DesignatedChannelType.MessageLog,
                    CreatedById = 1
                },
                new DesignatedChannelMappingCreationData()
                {
                    ChannelId = 3,
                    GuildId = 2,
                    Type = DesignatedChannelType.Unmoderated,
                    CreatedById = 3
                }
            };

        public static IEnumerable<(string name, DesignatedChannelMappingSearchCriteria? criteria, long[] resultIds)> Searches
            = new[]
            {
                (
                    "Null Criteria",
                    null,
                    new long[] { 1, 2, 3, 4 }
                ),
                (
                    "Empty Criteria",
                    new DesignatedChannelMappingSearchCriteria(),
                    new long[] { 1, 2, 3, 4 }
                ),
                (
                    "Id Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { Id = 1 },
                    new long[] { 1 }
                ),
                (
                    "Id Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { Id = 2 },
                    new long[] { 2 }
                ),
                (
                    "Id Invalid",
                    new DesignatedChannelMappingSearchCriteria() { Id = 5 },
                    new long[] { }
                ),
                (
                    "GuildId Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { GuildId = 1 },
                    new long[] { 1, 3 }
                ),
                (
                    "GuildId Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { GuildId = 2 },
                    new long[] { 2, 4 }
                ),
                (
                    "GuildId Invalid",
                    new DesignatedChannelMappingSearchCriteria() { GuildId = 3 },
                    new long[] { }
                ),
                (
                    "ChannelId Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { ChannelId = 1 },
                    new long[] { 1 }
                ),
                (
                    "ChannelId Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { ChannelId = 3 },
                    new long[] { 2, 4 }
                ),
                (
                    "ChannelId Invalid",
                    new DesignatedChannelMappingSearchCriteria() { ChannelId = 4 },
                    new long[] { }
                ),
                (
                    "Type Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.MessageLog },
                    new long[] { 1 }
                ),
                (
                    "Type Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.ModerationLog },
                    new long[] { 2, 3 }
                ),
                (
                    "Type Invalid",
                    new DesignatedChannelMappingSearchCriteria() { Type = DesignatedChannelType.Unmoderated },
                    new long[] { }
                ),
                (
                    "CreatedById Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { CreatedById = 1 },
                    new long[] { 1, 3 }
                ),
                (
                    "CreatedById Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { CreatedById = 2 },
                    new long[] { 4 }
                ),
                (
                    "CreatedById Invalid",
                    new DesignatedChannelMappingSearchCriteria() { CreatedById = 4 },
                    new long[] { }
                ),
                (
                    "IsDeleted Valid(1)",
                    new DesignatedChannelMappingSearchCriteria() { IsDeleted = true },
                    new long[] { 1 }
                ),
                (
                    "IsDeleted Valid(2)",
                    new DesignatedChannelMappingSearchCriteria() { IsDeleted = false },
                    new long[] { 2, 3, 4 }
                )
            };

        public static DesignatedChannelMappingEntity Clone(this DesignatedChannelMappingEntity entity)
            => new DesignatedChannelMappingEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                ChannelId = entity.ChannelId,
                CreateActionId = entity.CreateActionId,
                DeleteActionId = entity.DeleteActionId
            };

        public static IEnumerable<DesignatedChannelMappingEntity> Clone(this IEnumerable<DesignatedChannelMappingEntity> entities)
            => entities.Select(x => x.Clone());
    }
}
