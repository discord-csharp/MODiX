using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class Ban
    {
        [Key] public long BanID { get; set; }

        [NotMapped]
        public ulong DiscordBanID
        {
            get => (ulong) BanID;
            set => BanID = (long) value;
        }

        public long UserID { get; set; }

        [NotMapped]
        public ulong DiscordUserID
        {
            get => (ulong) UserID;
            set => UserID = (long) value;
        }

        public long CreatorID { get; set; }

        [NotMapped]
        public ulong DiscordCreatorID
        {
            get => (ulong) CreatorID;
            set => CreatorID = (long) value;
        }

        public DiscordGuild Guild { get; set; }
        public string Reason { get; set; }
        public bool Active { get; set; }
        public Infraction RelatedInfraction { get; set; }
    }
}