using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class Ban
    {
        [Key] public long BanId { get; set; }

        [NotMapped]
        public ulong DiscordBanId
        {
            get => (ulong) BanId;
            set => BanId = (long) value;
        }

        public long UserId { get; set; }

        [NotMapped]
        public ulong DiscordUserId
        {
            get => (ulong) UserId;
            set => UserId = (long) value;
        }

        public long CreatorId { get; set; }

        [NotMapped]
        public ulong DiscordCreatorId
        {
            get => (ulong) CreatorId;
            set => CreatorId = (long) value;
        }

        public DiscordGuild Guild { get; set; }
        public string Reason { get; set; }
        public bool Active { get; set; }
        public Infraction RelatedInfraction { get; set; }
    }
}