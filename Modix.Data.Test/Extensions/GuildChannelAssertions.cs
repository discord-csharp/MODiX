using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class GuildChannelAssertions
    {
        public static void ShouldMatchTestData(this GuildChannelBrief brief)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(GuildChannels.Entities.Select(x => x.ChannelId).ToArray());

            var entity = GuildChannels.Entities.First(x => x.ChannelId == brief.Id);

            brief.Name.ShouldBe(entity.Name);
        }

        public static void ShouldNotHaveChanged(this GuildChannelEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.ChannelId.ShouldBeOneOf(GuildChannels.Entities.Select(x => x.ChannelId).ToArray());

            var originalEntity = GuildChannels.Entities.First(x => x.ChannelId == entity.ChannelId);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.Name.ShouldBe(originalEntity.Name);
        }
    }
}
