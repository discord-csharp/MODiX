using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class GuildUserAssertions
    {
        public static void ShouldMatchTestData(this GuildUserBrief brief, ulong guildId)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(Users.Entities.Select(x => x.Id).ToArray());
            (brief.Id, guildId).ShouldBeOneOf(GuildUsers.Entities.Select(x => (x.UserId, x.GuildId)).ToArray());

            var userEntity = Users.Entities.First(x => x.Id == brief.Id);
            var guildUserEntity = GuildUsers.Entities.First(x => (x.UserId == brief.Id) && (x.GuildId == guildId));

            brief.Username.ShouldBe(userEntity.Username);
            brief.Discriminator.ShouldBe(userEntity.Discriminator);
            brief.Nickname.ShouldBe(guildUserEntity.Nickname);
        }

        public static void ShouldMatchTestData(this GuildUserSummary summary)
        {
            summary.ShouldNotBeNull();
            (summary.UserId, summary.GuildId).ShouldBeOneOf(GuildUsers.Entities.Select(x => (x.UserId, x.GuildId)).ToArray());

            var userEntity = Users.Entities.First(x => x.Id == summary.UserId);
            var guildUserEntity = GuildUsers.Entities.First(x => (x.UserId == summary.UserId) && (x.GuildId == summary.GuildId));

            summary.Username.ShouldBe(userEntity.Username);
            summary.Discriminator.ShouldBe(userEntity.Discriminator);
            summary.Nickname.ShouldBe(guildUserEntity.Nickname);
            summary.FirstSeen.ShouldBe(guildUserEntity.FirstSeen);
            summary.LastSeen.ShouldBe(guildUserEntity.LastSeen);
        }

        public static void ShouldNotHaveChanged(this GuildUserEntity entity)
        {
            entity.ShouldNotBeNull();
            (entity.UserId, entity.GuildId).ShouldBeOneOf(GuildUsers.Entities.Select(x => (x.UserId, x.GuildId)).ToArray());

            var originalEntity = GuildUsers.Entities.First(x => (x.UserId == entity.UserId) && (x.GuildId == entity.GuildId));

            entity.Nickname.ShouldBe(originalEntity.Nickname);
            entity.FirstSeen.ShouldBe(originalEntity.FirstSeen);
            entity.LastSeen.ShouldBe(originalEntity.LastSeen);
        }
    }
}
