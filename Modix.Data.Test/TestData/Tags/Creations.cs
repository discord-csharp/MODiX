using System.Collections.Generic;

using Modix.Data.Models.Tags;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Creations
    {
        public static readonly IEnumerable<TagCreationData> ValidCreations
            = new[]
            {
                new TagCreationData()
                {
                    GuildId = 1,
                    CreatedById = 1,
                    Name = "creation1",
                    Content = "Creation1Content",
                },
                new TagCreationData()
                {
                    GuildId = 2,
                    CreatedById = 3,
                    Name = "creation2",
                    Content = "Creation2Content",
                },
                new TagCreationData()
                {
                    GuildId = 1,
                    CreatedById = 1,
                    Name = "creation3",
                    Content = "Creation3Content1",
                },
                new TagCreationData()
                {
                    GuildId = 2,
                    CreatedById = 3,
                    Name = "creation3",
                    Content = "Creation3Content2",
                },
            };
    }
}
