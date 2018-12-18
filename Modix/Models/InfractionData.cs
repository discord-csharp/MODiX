using System;

using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Models
{
    public class InfractionData
    {
        public long Id { get; set; }

        public ulong GuildId { get; set; }

        public InfractionType Type { get; set; }

        public string Reason { get; set; }

        public TimeSpan? Duration { get; set; }

        public GuildUserBrief Subject { get; set; }

        public ModerationActionBrief CreateAction { get; set; }

        public ModerationActionBrief RescindAction { get; set; }

        public ModerationActionBrief DeleteAction { get; set; }

        public bool CanRescind { get; set; }

        public bool CanDelete { get; set; }
    }
}
