using System;

namespace Modix.Data.Models.Moderation
{
    public class InfractionData
    {
        public InfractionType Type { get; set; }

        public TimeSpan? Duration { get; set; }

        public long SubjectId { get; set; }

        public long CreateActionId { get; set; }

        internal InfractionEntity ToEntity()
            => new InfractionEntity()
            {
                Type = Type,
                Duration = Duration,
                SubjectId = SubjectId,
                CreateActionId = CreateActionId
            };
    }
}
