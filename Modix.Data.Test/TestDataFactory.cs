using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

using Newtonsoft.Json;

namespace Modix.Data.Test
{
    public static class TestDataFactory
    {
        public static async Task<IReadOnlyList<UserEntity>> BuildUsersAsync()
            => (await _lazyUsersAsync.Value)
                .Select(x => new UserEntity()
                {
                    Id = x.Id,
                    Username = x.Username,
                    Discriminator = x.Discriminator
                })
                .ToArray();

        private static readonly Lazy<Task<UserEntity[]>> _lazyUsersAsync
            = new Lazy<Task<UserEntity[]>>(async ()
                => JsonConvert.DeserializeObject<UserEntity[]>(
                    await File.ReadAllTextAsync(@"TestData/Users.json")));

        public static async Task<IReadOnlyList<GuildUserEntity>> BuildGuildUsersAsync()
            => (await _lazyGuildUsersAsync.Value)
                .Select(x => new GuildUserEntity()
                {
                    UserId = x.UserId,
                    GuildId = x.GuildId,
                    Nickname = x.Nickname,
                    FirstSeen = x.FirstSeen,
                    LastSeen = x.LastSeen
                })
                .ToArray();

        private static readonly Lazy<Task<GuildUserEntity[]>> _lazyGuildUsersAsync
            = new Lazy<Task<GuildUserEntity[]>>(async ()
                => JsonConvert.DeserializeObject<GuildUserEntity[]>(
                    await File.ReadAllTextAsync(@"TestData/GuildUsers.json")));
    }
}
