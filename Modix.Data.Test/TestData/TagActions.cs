using System;
using System.Collections.Generic;
using System.Linq;

using Modix.Data.Models.Tags;
using Modix.Data.Test.TestData.Tags;

namespace Modix.Data.Test.TestData
{
    internal static class TagActions
    {
        public static readonly IEnumerable<TagActionEntity> Entities
            = new[]
            {
                new TagActionEntity()
                {
                    Id = 1,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 1,
                    NewTagId = 1,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 2,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 1,
                    NewTagId = 2,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 3,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagDeleted,
                    CreatedById = 1,
                    NewTagId = null,
                    OldTagId = 2,
                },
                new TagActionEntity()
                {
                    Id = 4,
                    GuildId = 2,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 3,
                    NewTagId = 3,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 5,
                    GuildId = 2,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 3,
                    NewTagId = 4,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 6,
                    GuildId = 2,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagDeleted,
                    CreatedById = 3,
                    NewTagId = null,
                    OldTagId = 4,
                },
                new TagActionEntity()
                {
                    Id = 7,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 1,
                    NewTagId = 5,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 8,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagDeleted,
                    CreatedById = 1,
                    NewTagId = null,
                    OldTagId = 5,
                },
                new TagActionEntity()
                {
                    Id = 9,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 1,
                    NewTagId = 6,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 10,
                    GuildId = 1,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 1,
                    NewTagId = 7,
                    OldTagId = null,
                },
                new TagActionEntity()
                {
                    Id = 11,
                    GuildId = 2,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = 3,
                    NewTagId = 8,
                    OldTagId = null,
                },
            };

        public static TagActionEntity Clone(this TagActionEntity entity)
            => new TagActionEntity()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Created = entity.Created,
                Type = entity.Type,
                CreatedById = entity.CreatedById,
                CreatedBy = entity.CreatedBy?.Clone(),
                NewTagId = entity.NewTagId,
                NewTag = entity.NewTag?.Clone(),
                OldTagId = entity.OldTagId,
                OldTag = entity.OldTag?.Clone(),
            };

        public static IEnumerable<TagActionEntity> Clone(this IEnumerable<TagActionEntity> entities)
            => entities.Select(x => x.Clone());
    }
}
