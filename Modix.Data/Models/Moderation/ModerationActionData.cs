using System;

namespace Modix.Data.Models.Moderation
{
    public class ModerationActionData
    {
        public ModerationActionType Type { get; set; }

        public string Reason { get; set; }

        public long CreatedById { get; set; }

        public long? InfractionId { get; set; }

        internal ModerationActionEntity ToEntity()
            => new ModerationActionEntity()
            {
                Type = Type,
                Reason = Reason,
                CreatedById = CreatedById,
                InfractionId = InfractionId
            };
    }
}
