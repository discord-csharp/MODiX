using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class DesignatedChannelMappingAssertions
    {
        public static void ShouldMatchTestData(this DesignatedChannelMappingBrief brief)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(DesignatedChannelMappings.Entities.Select(x => x.Id).ToArray());

            var entity = DesignatedChannelMappings.Entities.First(x => x.Id == brief.Id);

            brief.Channel.ShouldMatchTestData();
            brief.Type.ShouldBe(entity.Type);
        }

        public static void ShouldNotHaveChanged(this DesignatedChannelMappingEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(DesignatedChannelMappings.Entities.Select(x => x.Id).ToArray());

            var originalEntity = DesignatedChannelMappings.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.ChannelId.ShouldBe(originalEntity.ChannelId);
            entity.Type.ShouldBe(originalEntity.Type);
            entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
            entity.DeleteActionId.ShouldBe(originalEntity.DeleteActionId);
        }
    }
}
