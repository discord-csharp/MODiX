using System.Collections.Generic;

using Modix.Data.Models.Emoji;

namespace Modix.Data.Test.TestData.Emoji
{
    internal static class Creations
    {
        public static readonly IEnumerable<EmojiCreationData> ValidCreations
            = new[]
            {
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 1,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    UsageType = EmojiUsageType.Reaction,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 2,
                    EmojiName = "emoji2",
                    IsAnimated = true,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 2,
                    EmojiName = "emoji2",
                    IsAnimated = true,
                    UsageType = EmojiUsageType.Reaction,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 3,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 3,
                    EmojiName = "emoji1",
                    IsAnimated = false,
                    UsageType = EmojiUsageType.Reaction,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 4,
                    EmojiName = "emoji2",
                    IsAnimated = true,
                    UsageType = EmojiUsageType.MessageContent,
                },
                new EmojiCreationData()
                {
                    GuildId = 1,
                    ChannelId = 1,
                    MessageId = 1,
                    UserId = 1,
                    EmojiId = 4,
                    EmojiName = "emoji2",
                    IsAnimated = true,
                    UsageType = EmojiUsageType.Reaction,
                },
            };
    }
}
