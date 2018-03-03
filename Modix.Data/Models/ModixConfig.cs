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
    }
}
