using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Tags;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Tags
    {
        public static readonly IEnumerable<TagEntity> Entities
            = new[]
            {
                new TagEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    CreateActionId = 1,
                    DeleteActionId = null,
                    Name = "created",
                    Content = "CreatedTagContent",
                    Uses = 0,
                },
                new TagEntity()
                {
                    Id = 2,
                    GuildId = 1,
                    CreateActionId = 2,
                    DeleteActionId = 3,
                    Name = "deleted",
                    Content = "DeletedTagContent",
                    Uses = 1,
                },
                new TagEntity()
                {
                    Id = 3,
                    GuildId = 2,
                    CreateActionId = 4,
                    DeleteActionId = null,
                    Name = "otherguildcreated",
                    Content = "OtherGuildCreatedTagContent",
                    Uses = 2,
                },
                new TagEntity()
                {
                    Id = 4,
                    GuildId = 2,
                    CreateActionId = 5,
                    DeleteActionId = 6,
                    Name = "otherguilddeleted",
                    Content = "OtherGuildDeletedTagContent",
                    Uses = 4,
                },
                new TagEntity()
                {
                    Id = 5,
                    GuildId = 1,
                    CreateActionId = 7,
                    DeleteActionId = 8,
                    Name = "recreated",
                    Content = "RecreatedTagContent",
                    Uses = 8,
                },
                new TagEntity()
                {
                    Id = 6,
                    GuildId = 1,
                    CreateActionId = 9,
                    DeleteActionId = null,
                    Name = "recreated",
                    Content = "RecreatedTagContent",
                    Uses = 16,
                },
                new TagEntity()
                {
                    Id = 7,
                    GuildId = 1,
                    CreateActionId = 10,
                    DeleteActionId = null,
                    Name = "sametagacrossguilds",
                    Content = "SameTagAcrossGuildsContent1",
                    Uses = 32,
                },
                new TagEntity()
                {
                    Id = 8,
                    GuildId = 2,
                    CreateActionId = 11,
                    DeleteActionId = null,
                    Name = "sametagacrossguilds",
                    Content = "SameTagAcrossGuildsContent2",
                    Uses = 64,
                },
            };

        public static TagEntity Clone(this TagEntity entity)
            => new TagEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                CreateActionId = entity.CreateActionId,
                CreateAction = entity.CreateAction?.Clone(),
                DeleteActionId = entity.DeleteActionId,
                DeleteAction = entity.DeleteAction?.Clone(),
                Name = entity.Name,
                Content = entity.Content,
                Uses = entity.Uses,
            };

        public static IEnumerable<TagEntity> Clone(this IEnumerable<TagEntity> entities)
            => entities.Select(x => x.Clone());
    }
}
