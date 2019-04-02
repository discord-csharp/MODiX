namespace Modix.Data.Models.Core
{
    public class PerUserMessageCount
    {
        public string Username { get; set; }

        public string Discriminator { get; set; }

        public int Rank { get; set; }

        public int MessageCount { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}
