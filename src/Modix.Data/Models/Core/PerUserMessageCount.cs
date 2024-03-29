namespace Modix.Data.Models.Core
{
    public class PerUserMessageCount
    {
        public string Username { get; set; } = null!;

        public string Discriminator { get; set; } = null!;

        public int Rank { get; set; }

        public int MessageCount { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}
