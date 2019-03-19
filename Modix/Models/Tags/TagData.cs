using System;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;

namespace Modix.Models.Tags
{
    public class TagData
    {
        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public bool IsOwnedByRole { get; set; }

        public GuildUserBrief OwnerUser { get; set; }

        public GuildRoleBrief OwnerRole { get; set; }

        public string Content { get; set; }

        public uint Uses { get; set; }

        public bool CanMaintain { get; set; }

        internal TagSummary TagSummary { get; set; }
    }
}
