using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Emoji;

namespace Modix.Data.Test.TestData.Emoji
{
    internal static class Emoji
    {
        public static readonly IEnumerable<EmojiEntity> Entities
            = new EmojiEntity[]
            {
                new EmojiEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 2,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow,
                    UsageType = EmojiUsageType.Reaction,
                },
                new EmojiEntity()
                {
                    Id = 3,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 4,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 5,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = null,
                    EmojiName = "emoji2",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 6,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 2,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 7,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 2,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow,
                    UsageType = EmojiUsageType.Reaction,
                },
                new EmojiEntity()
                {
                    Id = 8,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 3,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 9,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 3,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiEntity()
                {
                    Id = 10,
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 3,
                    EmojiId = null,
                    EmojiName = "emoji2",
                    IsAnimated = false,
                    Timestamp = DateTimeOffset.UtcNow - TimeSpan.FromDays(29),
                    UsageType = EmojiUsageType.MessageContent,
                },
            };

        public static EmojiEntity Clone(this EmojiEntity entity)
            => new EmojiEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                ChannelId = entity.ChannelId,
                MessageId = entity.MessageId,
                UserId = entity.UserId,
                EmojiId = entity.EmojiId,
                EmojiName = entity.EmojiName,
                IsAnimated = entity.IsAnimated,
                Timestamp = entity.Timestamp,
                UsageType = entity.UsageType,
            };

        public static IEnumerable<EmojiEntity> Clone(this IEnumerable<EmojiEntity> entities)
            => entities.Select(x => x.Clone());
    }
}
