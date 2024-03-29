using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models;
using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class ClaimMappings
    {
        public static readonly IEnumerable<ClaimMappingEntity> Entities
            = new[]
            {
                new ClaimMappingEntity()
                {
                    Id = 1,
                    Type = ClaimMappingType.Granted,
                    GuildId = 1,
                    RoleId = 1,
                    UserId = null,
                    Claim = AuthorizationClaim.AuthorizationConfigure,
                    CreateActionId = 1,
                    DeleteActionId = 2,
                },
                new ClaimMappingEntity()
                {
                    Id = 2,
                    Type = ClaimMappingType.Denied,
                    GuildId = 2,
                    RoleId = 2,
                    UserId = null,
                    Claim = AuthorizationClaim.DesignatedChannelMappingCreate,
                    CreateActionId = 3,
                    DeleteActionId = null,
                },
                new ClaimMappingEntity()
                {
                    Id = 3,
                    Type = ClaimMappingType.Denied,
                    GuildId = 1,
                    RoleId = null,
                    UserId = 1,
                    Claim = AuthorizationClaim.DesignatedChannelMappingDelete,
                    CreateActionId = 4,
                    DeleteActionId = null,
                },
                new ClaimMappingEntity()
                {
                    Id = 4,
                    Type = ClaimMappingType.Granted,
                    GuildId = 1,
                    RoleId = null,
                    UserId = 2,
                    Claim = AuthorizationClaim.AuthorizationConfigure,
                    CreateActionId = 5,
                    DeleteActionId = null,
                }
            };

        public static IEnumerable<ClaimMappingCreationData> Creations
            = new[]
            {
                new ClaimMappingCreationData()
                {
                    GuildId = 1,
                    Type = ClaimMappingType.Granted,
                    RoleId = 3,
                    UserId = null,
                    Claim = AuthorizationClaim.ModerationBan,
                    CreatedById = 1
                },
                new ClaimMappingCreationData()
                {
                    GuildId = 2,
                    Type = ClaimMappingType.Denied,
                    RoleId = null,
                    UserId = 3,
                    Claim = AuthorizationClaim.ModerationConfigure,
                    CreatedById = 2
                }
            };

        public static IEnumerable<(string name, ClaimMappingSearchCriteria? criteria, long[] resultIds)> Searches
            = new[]
            {
                (
                    "Null Criteria",
                    null,
                    new long [] { 1, 2, 3, 4 }
                ),
                (
                    "Empty Criteria",
                    new ClaimMappingSearchCriteria() { },
                    new long [] { 1, 2, 3, 4 }
                ),
                (
                    "Empty Types",
                    new ClaimMappingSearchCriteria() { Types = new ClaimMappingType[] { } },
                    new long [] { 1, 2, 3, 4 }
                ),
                (
                    "Types Valid (1)",
                    new ClaimMappingSearchCriteria() { Types = new[] { ClaimMappingType.Granted } },
                    new long [] { 1, 4 }
                ),
                (
                    "Types Valid (2)",
                    new ClaimMappingSearchCriteria() { Types = new[] { ClaimMappingType.Denied } },
                    new long [] { 2, 3 }
                ),
                (
                    "Types Valid(multiple)",
                    new ClaimMappingSearchCriteria() { Types = new[] { ClaimMappingType.Granted, ClaimMappingType.Denied } },
                    new long [] { 1, 2, 3, 4 }
                ),
                (
                    "Types Invalid",
                    new ClaimMappingSearchCriteria() { Types = new[] { (ClaimMappingType)(-1) } },
                    new long [] { }
                ),
                (
                    "GuildId Valid(1)",
                    new ClaimMappingSearchCriteria() { GuildId = 1 },
                    new long [] { 1, 3, 4 }
                ),
                (
                    "GuildId Valid(2)",
                    new ClaimMappingSearchCriteria() { GuildId = 2 },
                    new long [] { 2 }
                ),
                (
                    "GuildId Invalid",
                    new ClaimMappingSearchCriteria() { GuildId = 3 },
                    new long [] { }
                ),
                (
                    "RoleIds Valid(1)",
                    new ClaimMappingSearchCriteria() { RoleIds = new ulong[] { 1 } },
                    new long [] { 1 }
                ),
                (
                    "RoleIds Valid(2)",
                    new ClaimMappingSearchCriteria() { RoleIds = new ulong[] { 2 } },
                    new long [] { 2 }
                ),
                (
                    "RoleIds Vaiid(multiple)",
                    new ClaimMappingSearchCriteria() { RoleIds = new ulong[] { 1, 2 } },
                    new long [] { 1, 2 }
                ),
                (
                    "RoleIds Invalid",
                    new ClaimMappingSearchCriteria() { RoleIds = new ulong[] { 3 } },
                    new long [] { }
                ),
                (
                    "UserId Valid(1)",
                    new ClaimMappingSearchCriteria() { UserId = 1 },
                    new long [] { 3 }
                ),
                (
                    "UserId Valid(2)",
                    new ClaimMappingSearchCriteria() { UserId = 2 },
                    new long [] { 4 }
                ),
                (
                    "UserId Invalid",
                    new ClaimMappingSearchCriteria() { UserId = 3 },
                    new long [] { }
                ),
                (
                    "RoleIds and UserId valid",
                    new ClaimMappingSearchCriteria() { RoleIds = new ulong[] { 1, 2 }, UserId = 1 },
                    new long [] { 1, 2, 3 }
                ),
                (
                    "Claims Valid(1)",
                    new ClaimMappingSearchCriteria() { Claims = new [] { AuthorizationClaim.AuthorizationConfigure } },
                    new long [] { 1, 4 }
                ),
                (
                    "Claims Valid(2)",
                    new ClaimMappingSearchCriteria() { Claims = new [] { AuthorizationClaim.DesignatedChannelMappingCreate } },
                    new long [] { 2 }
                ),
                (
                    "Claims Valid(multiple)",
                    new ClaimMappingSearchCriteria() { Claims = new [] { AuthorizationClaim.AuthorizationConfigure, AuthorizationClaim.DesignatedChannelMappingCreate } },
                    new long [] { 1, 2, 4 }
                ),
                (
                    "Claims Invalid",
                    new ClaimMappingSearchCriteria() { Claims = new [] { AuthorizationClaim.PromotionsCreateCampaign } },
                    new long [] { }
                ),
                (
                    "CreatedRange.From Valid",
                    new ClaimMappingSearchCriteria() { CreatedRange = new DateTimeOffsetRange() { From = DateTimeOffset.Parse("2018-01-04T00:00:00Z") } },
                    new long [] { 3, 4 }
                ),
                (
                    "CreatedRange.To Valid",
                    new ClaimMappingSearchCriteria() { CreatedRange = new DateTimeOffsetRange() { To = DateTimeOffset.Parse("2018-01-04T00:00:00Z") } },
                    new long [] { 1, 2, 3 }
                ),
                (
                    "CreatedRange.From and .To Valid",
                    new ClaimMappingSearchCriteria() { CreatedRange = new DateTimeOffsetRange() { From = DateTimeOffset.Parse("2018-01-02T00:00:00Z"), To = DateTimeOffset.Parse("2018-01-04T00:00:00Z") } },
                    new long [] { 2, 3 }
                ),
                (
                    "CreatedRange.From Invalid",
                    new ClaimMappingSearchCriteria() { CreatedRange = new DateTimeOffsetRange() { From = DateTimeOffset.Parse("2019-01-01T00:00:00Z") } },
                    new long [] { }
                ),
                (
                    "CreatedRange.To Invalid",
                    new ClaimMappingSearchCriteria() { CreatedRange = new DateTimeOffsetRange() { To = DateTimeOffset.Parse("2017-01-01T00:00:00Z") } },
                    new long [] { }
                ),
                (
                    "CreatedById Valid(1)",
                    new ClaimMappingSearchCriteria() { CreatedById = 1 },
                    new long [] { 1, 3 }
                ),
                (
                    "CreatedById Valid(2)",
                    new ClaimMappingSearchCriteria() { CreatedById = 2 },
                    new long [] { 4 }
                ),
                (
                    "CreatedById Invalid",
                    new ClaimMappingSearchCriteria() { CreatedById = 4 },
                    new long [] { }
                ),
                (
                    "IsDeleted Valid(1)",
                    new ClaimMappingSearchCriteria() { IsDeleted = true },
                    new long [] { 1 }
                ),
                (
                    "IsDeleted Valid(2)",
                    new ClaimMappingSearchCriteria() { IsDeleted = false },
                    new long [] { 2, 3, 4 }
                ),
            };

        public static ClaimMappingEntity Clone(this ClaimMappingEntity entity)
            => new ClaimMappingEntity()
            {
                Id = entity.Id,
                Type = entity.Type,
                GuildId = entity.GuildId,
                RoleId = entity.RoleId,
                UserId = entity.UserId,
                Claim = entity.Claim,
                CreateActionId = entity.CreateActionId,
                DeleteActionId = entity.DeleteActionId
            };

        public static IEnumerable<ClaimMappingEntity> Clone(this IEnumerable<ClaimMappingEntity> entities)
            => entities.Select(Clone);
    }
}
