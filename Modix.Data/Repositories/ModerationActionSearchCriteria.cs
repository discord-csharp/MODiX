using System;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    public class ModerationActionSearchCriteria
    {
        public ModerationActionTypes[] Types { get; set; }

        public ulong? InfractionId { get; set; }

        public DateTimeOffset? CreatedFrom { get; set; }

        public DateTimeOffset? CreatedTo { get; set; }

        public ulong? CreatedById { get; set; }
    }
}
