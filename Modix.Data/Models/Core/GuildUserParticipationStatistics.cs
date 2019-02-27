namespace Modix.Data.Models.Core
{
    public class GuildUserParticipationStatistics
    {
        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public int Rank { get; set; }

        public decimal AveragePerDay { get; set; }

        public int Percentile { get; set; }
    }
}
