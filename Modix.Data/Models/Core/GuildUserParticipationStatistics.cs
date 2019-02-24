using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Data.Models.Core
{
    public class GuildUserParticipationStatistics
    {
        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public int Rank { get; set; }

        public decimal AveragePerDay { get; set; }

        public int Percentile { get; set; }

        public string GetParticipationEmoji()
        {
            if (Percentile == 100 || Rank == 1)
            {
                return "🥇";
            }
            else if (Percentile == 99 || Rank == 2)
            {
                return "🥈";
            }
            else if (Percentile == 98 || Rank == 3)
            {
                return "🥉";
            }
            else if (Percentile > 95 && Percentile < 98)
            {
                return "🏆";
            }

            return string.Empty;
        }
    }
}
