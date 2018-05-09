namespace Modix.Data.Models
{
    public class ModixConfig
    {
        public string DiscordToken { get; set; }
        public string StackoverflowToken { get; set; }
        public string ReplToken { get; set; }
        public string PostgreConnectionString { get; set; }
        public string WebhookToken { get; set; }
        public ulong WebhookId { get; set; }
        public string SentryToken { get; set; }
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string AlphaVantageToken { get; set; }
    }
}
