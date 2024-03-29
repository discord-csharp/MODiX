using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class GuildUsers
    {
        public static readonly IEnumerable<GuildUserEntity> Entities
            = new GuildUserEntity[]
            {
                new GuildUserEntity()
                {
                    UserId = 1,
                    GuildId = 1,
                    Nickname = "Nickname1",
                    FirstSeen = DateTimeOffset.Parse("2018-01-01T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-01-02T00:00:00Z")
                },
                new GuildUserEntity()
                {
                    UserId = 2,
                    GuildId = 1,
                    Nickname = "Nickname2",
                    FirstSeen = DateTimeOffset.Parse("2018-01-03T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-01-04T00:00:00Z")
                },
                new GuildUserEntity()
                {
                    UserId = 3,
                    GuildId = 1,
                    Nickname = "Nickname3",
                    FirstSeen = DateTimeOffset.Parse("2018-01-05T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-01-06T00:00:00Z")
                },
                new GuildUserEntity()
                {
                    UserId = 3,
                    GuildId = 2,
                    Nickname = "Nickname4",
                    FirstSeen = DateTimeOffset.Parse("2018-01-07T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-01-08T00:00:00Z")
                }
            };

        public static GuildUserEntity Clone(this GuildUserEntity entity)
            => new GuildUserEntity()
            {
                UserId = entity.UserId,
                GuildId = entity.GuildId,
                Nickname = entity.Nickname,
                FirstSeen = entity.FirstSeen,
                LastSeen = entity.LastSeen
            };

        public static IEnumerable<GuildUserEntity> Clone(this IEnumerable<GuildUserEntity> entities)
            => entities.Select(Clone);

        public static readonly IEnumerable<GuildUserCreationData> NewCreations
            = new GuildUserCreationData[]
            {
                new GuildUserCreationData()
                {
                    UserId = 4,
                    GuildId = 1,
                    Username = "NewUsername",
                    Discriminator = "0000",
                    Nickname = "NewNickname2",
                    FirstSeen = DateTimeOffset.Parse("2018-02-03T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-02-04T00:00:00Z")
                }
            };

        public static readonly IEnumerable<GuildUserCreationData> ExistingCreations
            = new GuildUserCreationData[]
            {
                new GuildUserCreationData()
                {
                    UserId = 1,
                    GuildId = 1,
                    Nickname = "ExistingNickname1",
                    FirstSeen = DateTimeOffset.Parse("2018-03-01T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-03-02T00:00:00Z")
                },
                new GuildUserCreationData()
                {
                    UserId = 2,
                    GuildId = 1,
                    Nickname = "ExistingNickname2",
                    FirstSeen = DateTimeOffset.Parse("2018-03-03T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-03-04T00:00:00Z")
                },
                new GuildUserCreationData()
                {
                    UserId = 3,
                    GuildId = 1,
                    Nickname = "ExistingNickname3",
                    FirstSeen = DateTimeOffset.Parse("2018-03-05T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-03-06T00:00:00Z")
                },
                new GuildUserCreationData()
                {
                    UserId = 3,
                    GuildId = 2,
                    Nickname = "ExistingNickname4",
                    FirstSeen = DateTimeOffset.Parse("2018-03-07T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-03-08T00:00:00Z")
                }
            };
    }
}
