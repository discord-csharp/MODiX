namespace Modix.Data.Models
{
    public class Ban
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public long CreatorId { get; set; }
        public DiscordGuild Guild { get; set; }
        public string Reason { get; set; }
        public bool Active { get; set; }
    }
}
