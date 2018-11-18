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
            brief.Id.ShouldBeOneOf(GuildUsers.Entities.Select(x => x.UserId).ToArray());

            var userEntity = Users.Entities.First(x => x.Id == brief.Id);
            var guildUserEntity = GuildUsers.Entities.First(x => (x.UserId == brief.Id) && (x.GuildId == guildId));

            brief.Username.ShouldBe(userEntity.Username);
            brief.Discriminator.ShouldBe(userEntity.Discriminator);
            brief.Nickname.ShouldBe(guildUserEntity.Nickname);
        }
    }
}
