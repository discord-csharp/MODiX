using System;
using System.Linq;
using System.Collections.Generic;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class DesignatedRoleMappings
    {
        public static readonly IEnumerable<DesignatedRoleMappingEntity> Entities
            = new[]
            {
                new DesignatedRoleMappingEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    Type = DesignatedRoleType.ModerationMute,
                    RoleId = 1,
                    CreateActionId = 11,
                    DeleteActionId = 12
                },
                new DesignatedRoleMappingEntity()
                {
                    Id = 2,
                    GuildId = 2,
                    Type = DesignatedRoleType.Rank,
                    RoleId = 3,
                    CreateActionId = 13,
                    DeleteActionId = null
                },
                new DesignatedRoleMappingEntity()
                {
                    Id = 3,
                    GuildId = 1,
                    Type = DesignatedRoleType.Rank,
                    RoleId = 2,
                    CreateActionId = 14,
                    DeleteActionId = null
                },
                new DesignatedRoleMappingEntity()
                {
                    Id = 4,
                    GuildId = 2,
                    Type = DesignatedRoleType.ModerationMute,
                    RoleId = 3,
                    CreateActionId = 15,
                    DeleteActionId = null
                }
            };

        public static readonly IEnumerable<DesignatedRoleMappingCreationData> Creations
            = new[]
            {
                new DesignatedRoleMappingCreationData()
                {
                    RoleId = 2,
                    GuildId = 1,
                    Type = DesignatedRoleType.ModerationMute,
                    CreatedById = 1
                },
                new DesignatedRoleMappingCreationData()
                {
                    RoleId = 3,
                    GuildId = 2,
                    Type = DesignatedRoleType.Rank,
                    CreatedById = 3
                }
            };

        public static IEnumerable<(string name, DesignatedRoleMappingSearchCriteria? criteria, long[] resultIds)> Searches
            = new[]
            {
                (
                    "Null Criteria",
                    null,
                    new long[] { 1, 2, 3, 4 }
                ),
                (
                    "Empty Criteria",
                    new DesignatedRoleMappingSearchCriteria(),
                    new long[] { 1, 2, 3, 4 }
                ),
                (
                    "Id Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { Id = 1 },
                    new long[] { 1 }
                ),
                (
                    "Id Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { Id = 2 },
                    new long[] { 2 }
                ),
                (
                    "Id Invalid",
                    new DesignatedRoleMappingSearchCriteria() { Id = 5 },
                    new long[] { }
                ),
                (
                    "GuildId Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { GuildId = 1 },
                    new long[] { 1, 3 }
                ),
                (
                    "GuildId Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { GuildId = 2 },
                    new long[] { 2, 4 }
                ),
                (
                    "GuildId Invalid",
                    new DesignatedRoleMappingSearchCriteria() { GuildId = 3 },
                    new long[] { }
                ),
                (
                    "RoleId Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { RoleId = 1 },
                    new long[] { 1 }
                ),
                (
                    "RoleId Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { RoleId = 3 },
                    new long[] { 2, 4 }
                ),
                (
                    "RoleId Invalid",
                    new DesignatedRoleMappingSearchCriteria() { RoleId = 4 },
                    new long[] { }
                ),
                (
                    "RoleIds Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { RoleIds = new[] { 1UL } },
                    new long[] { 1 }
                ),
                (
                    "RoleIds Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { RoleIds = new[] { 3UL } },
                    new long[] { 2, 4 }
                ),
                (
                    "RoleIds Valid(3)",
                    new DesignatedRoleMappingSearchCriteria() { RoleIds = new[] { 1UL, 3UL } },
                    new long[] { 1, 2, 4 }
                ),
                (
                    "RoleIds Invalid(1)",
                    new DesignatedRoleMappingSearchCriteria() { RoleIds = Array.Empty<ulong>() },
                    new long[] { }
                ),
                (
                    "RoleIds Invalid(2)",
                    new DesignatedRoleMappingSearchCriteria() { RoleIds = new[] { 4UL } },
                    new long[] { }
                ),
                (
                    "Type Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { Type = DesignatedRoleType.ModerationMute },
                    new long[] { 1, 4 }
                ),
                (
                    "Type Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { Type = DesignatedRoleType.Rank },
                    new long[] { 2, 3 }
                ),
                (
                    "Type Invalid",
                    new DesignatedRoleMappingSearchCriteria() { Type = (DesignatedRoleType)(-1) },
                    new long[] { }
                ),
                (
                    "CreatedById Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { CreatedById = 1 },
                    new long[] { 1, 3 }
                ),
                (
                    "CreatedById Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { CreatedById = 2 },
                    new long[] { 4 }
                ),
                (
                    "CreatedById Invalid",
                    new DesignatedRoleMappingSearchCriteria() { CreatedById = 4 },
                    new long[] { }
                ),
                (
                    "IsDeleted Valid(1)",
                    new DesignatedRoleMappingSearchCriteria() { IsDeleted = true },
                    new long[] { 1 }
                ),
                (
                    "IsDeleted Valid(2)",
                    new DesignatedRoleMappingSearchCriteria() { IsDeleted = false },
                    new long[] { 2, 3, 4 }
                )
            };

        public static DesignatedRoleMappingEntity Clone(this DesignatedRoleMappingEntity entity)
            => new DesignatedRoleMappingEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                RoleId = entity.RoleId,
                CreateActionId = entity.CreateActionId,
                DeleteActionId = entity.DeleteActionId
            };

        public static IEnumerable<DesignatedRoleMappingEntity> Clone(this IEnumerable<DesignatedRoleMappingEntity> entities)
            => entities.Select(x => x.Clone());
    }
}
