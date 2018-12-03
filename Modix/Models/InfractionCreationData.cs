namespace Modix.Models
{
    public class InfractionCreationData
    {
        public string Type { get; set; }

        public string Reason { get; set; }

        public int? DurationMonths { get; set; }

        public int? DurationDays { get; set; }

        public int? DurationHours { get; set; }

        public int? DurationMinutes { get; set; }

        public int? DurationSeconds { get; set; }
    }
}
