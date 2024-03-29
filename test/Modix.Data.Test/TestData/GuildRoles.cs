using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Core;

namespace Modix.Data.Test.TestData
{
    public static class GuildRoles
    {
        public static readonly IEnumerable<GuildRoleEntity> Entities
            = new GuildRoleEntity[]
            {
                new GuildRoleEntity()
                {
                    RoleId = 1,
                    GuildId = 1,
                    Name = "GuildRole1",
                    Position = 10
                },
                new GuildRoleEntity()
                {
                    RoleId = 2,
                    GuildId = 1,
                    Name = "GuildRole2",
                    Position = 11
                },
                new GuildRoleEntity()
                {
                    RoleId = 3,
                    GuildId = 2,
                    Name = "GuildRole3",
                    Position = 12
                }
            };

        public static GuildRoleEntity Clone(this GuildRoleEntity entity)
            => new GuildRoleEntity()
            {
                RoleId = entity.RoleId,
                GuildId = entity.GuildId,
                Name = entity.Name,
                Position = entity.Position
            };

        public static IEnumerable<GuildRoleEntity> Clone(this IEnumerable<GuildRoleEntity> entities)
            => entities.Select(Clone);

        public static readonly IEnumerable<GuildRoleCreationData> NewCreations
            = new GuildRoleCreationData[]
            {
                new GuildRoleCreationData()
                {
                    RoleId = 4,
                    GuildId = 3,
                    Name = "NewGuildRole4",
                    Position = 13
                }
            };

        public static readonly IEnumerable<GuildRoleCreationData> ExistingCreations
            = new GuildRoleCreationData[]
            {
                new GuildRoleCreationData()
                {
                    RoleId = 1,
                    GuildId = 1,
                    Name = "ExistingGuildRole1",
                    Position = 14
                },
                new GuildRoleCreationData()
                {
                    RoleId = 2,
                    GuildId = 1,
                    Name = "ExistingGuildRole2",
                    Position = 15
                },
                new GuildRoleCreationData()
                {
                    RoleId = 3,
                    GuildId = 2,
                    Name = "ExistingGuildRole3",
                    Position = 16
                },
                new GuildRoleCreationData()
                {
                    RoleId = 3,
                    GuildId = 3,
                    Name = "ExistingGuildRole4",
                    Position = 17
                }
            };
    }
}
