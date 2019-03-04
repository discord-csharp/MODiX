namespace Modix.Data.Models.Core
{
    public class ModixConfig
    {
        public string DiscordToken { get; set; }

        public string StackoverflowToken { get; set; }

        public string DbConnection { get; set; }

        public string LogWebhookToken { get; set; }

        public ulong LogWebhookId { get; set; }

        public string DiscordClientId { get; set; }

        public string DiscordClientSecret { get; set; }

        public int MessageCacheSize { get; set; } = 10;
    }
}
