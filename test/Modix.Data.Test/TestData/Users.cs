using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class Users
    {
        public static readonly IEnumerable<UserEntity> Entities
            = new UserEntity[]
            {
                new UserEntity()
                {
                    Id = 1,
                    Username = "Username1",
                    Discriminator = "Discriminator1"
                },
                new UserEntity()
                {
                    Id = 2,
                    Username = "Username2",
                    Discriminator = "Discriminator2"
                },
                new UserEntity()
                {
                    Id = 3,
                    Username = "Username3",
                    Discriminator = "Discriminator3"
                },
            };

        public static UserEntity Clone(this UserEntity entity)
            => new UserEntity()
            {
                Id = entity.Id,
                Username = entity.Username,
                Discriminator = entity.Discriminator
            };

        public static IEnumerable<UserEntity> Clone(this IEnumerable<UserEntity> entities)
            => entities.Select(Clone);

        public static readonly IEnumerable<GuildUserCreationData> NewCreations
            = new GuildUserCreationData[]
            {
                new GuildUserCreationData()
                {
                    UserId = 4,
                    GuildId = 1,
                    Username = "NewUsername1",
                    Discriminator = "NewDiscriminator1",
                    Nickname = "NewNickname1",
                    FirstSeen = DateTimeOffset.Parse("2018-02-01T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-02-02T00:00:00Z")
                }
            };

        public static readonly IEnumerable<GuildUserCreationData> ExistingCreations
            = new GuildUserCreationData[]
            {
                new GuildUserCreationData()
                {
                    UserId = 1,
                    GuildId = 2,
                    Username = "ExistingUsername1",
                    Discriminator = "ExistingDiscriminator1",
                    Nickname = "ExistingNickname2",
                    FirstSeen = DateTimeOffset.Parse("2018-02-03T00:00:00Z"),
                    LastSeen = DateTimeOffset.Parse("2018-02-04T00:00:00Z")
                }
            };

    }
}
