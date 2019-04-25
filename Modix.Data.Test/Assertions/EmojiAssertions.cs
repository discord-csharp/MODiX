using System.Linq;

using Modix.Data.Models.Emoji;
using Modix.Data.Test.TestData.Emoji;

namespace Shouldly
{
    internal static class EmojiAssertions
    {
        public static void ShouldNotHaveChanged(this EmojiEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(Emoji.Entities.Select(x => x.Id).ToArray());

            var originalEntity = Emoji.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.ChannelId.ShouldBe(originalEntity.ChannelId);
            entity.MessageId.ShouldBe(originalEntity.MessageId);
            entity.UserId.ShouldBe(originalEntity.UserId);
            entity.EmojiId.ShouldBe(originalEntity.EmojiId);
            entity.EmojiName.ShouldBe(originalEntity.EmojiName);
            entity.IsAnimated.ShouldBe(originalEntity.IsAnimated);
            entity.Timestamp.ShouldBe(originalEntity.Timestamp);
            entity.UsageType.ShouldBe(originalEntity.UsageType);
        }
    }
}
