using System;
using System.Collections.Generic;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    public class InfractionSearchCriteria
    {
        public IEnumerable<InfractionTypes> Types { get; set; }

        public ulong? SubjectId { get; set; }

        public DateTimeOffset? CreatedFrom { get; set; }

        public DateTimeOffset? CreatedTo { get; set; }

        public ulong? CreatedById { get; set; }

        public bool? IsExpired { get; set; }

        public bool? IsRescinded { get; set; }
    }
}
