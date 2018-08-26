namespace Modix.Data.Models.Core
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

        public int MessageCacheSize { get; set; } = 10;

        // Website Authentication Error - Modify "X" for your server if desired.
        public string WebAuthenticationError { get; set; } = "You must be a member of the Discord X server to log in.";
    }
}