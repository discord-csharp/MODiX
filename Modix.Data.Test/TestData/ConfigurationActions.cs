using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class ConfigurationActions
    {
        public static readonly IEnumerable<ConfigurationActionEntity> Entities
            = new[]
            {
                new ConfigurationActionEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    Type = ConfigurationActionType.ClaimMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-01T00:00:00Z"),
                    CreatedById = 1,
                    ClaimMappingId = 1
                },
                new ConfigurationActionEntity()
                {
                    Id = 2,
                    GuildId = 1,
                    Type = ConfigurationActionType.ClaimMappingDeleted,
                    Created = DateTimeOffset.Parse("2018-01-02T00:00:00Z"),
                    CreatedById = 2,
                    ClaimMappingId = 1
                },
                new ConfigurationActionEntity()
                {
                    Id = 3,
                    GuildId = 2,
                    Type = ConfigurationActionType.ClaimMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-03T00:00:00Z"),
                    CreatedById = 3,
                    ClaimMappingId = 2
                },
                new ConfigurationActionEntity()
                {
                    Id = 4,
                    GuildId = 1,
                    Type = ConfigurationActionType.ClaimMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-04T00:00:00Z"),
                    CreatedById = 1,
                    ClaimMappingId = 3
                },
                new ConfigurationActionEntity()
                {
                    Id = 5,
                    GuildId = 1,
                    Type = ConfigurationActionType.ClaimMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-05T00:00:00Z"),
                    CreatedById = 2,
                    ClaimMappingId = 4
                },
                new ConfigurationActionEntity()
                {
                    Id = 6,
                    GuildId = 1,
                    Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-06T00:00:00Z"),
                    CreatedById = 1,
                    DesignatedChannelMappingId = 1
                },
                new ConfigurationActionEntity()
                {
                    Id = 7,
                    GuildId = 1,
                    Type = ConfigurationActionType.DesignatedChannelMappingDeleted,
                    Created = DateTimeOffset.Parse("2018-01-07T00:00:00Z"),
                    CreatedById = 2,
                    DesignatedChannelMappingId = 1
                },
                new ConfigurationActionEntity()
                {
                    Id = 8,
                    GuildId = 2,
                    Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-08T00:00:00Z"),
                    CreatedById = 3,
                    DesignatedChannelMappingId = 2
                },
                new ConfigurationActionEntity()
                {
                    Id = 9,
                    GuildId = 1,
                    Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-09T00:00:00Z"),
                    CreatedById = 1,
                    DesignatedChannelMappingId = 3
                },
                new ConfigurationActionEntity()
                {
                    Id = 10,
                    GuildId = 1,
                    Type = ConfigurationActionType.DesignatedChannelMappingCreated,
                    Created = DateTimeOffset.Parse("2018-01-10T00:00:00Z"),
                    CreatedById = 2,
                    DesignatedChannelMappingId = 4
                }
            };

        public static ConfigurationActionEntity Clone(this ConfigurationActionEntity entity)
            => new ConfigurationActionEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                Created = entity.Created,
                CreatedById = entity.CreatedById,
                ClaimMappingId = entity.ClaimMappingId,
                DesignatedChannelMappingId = entity.DesignatedChannelMappingId,
                DesignatedRoleMappingId = entity.DesignatedRoleMappingId
            };

        public static IEnumerable<ConfigurationActionEntity> Clone(this IEnumerable<ConfigurationActionEntity> entities)
            => entities.Select(Clone);
    }
}
