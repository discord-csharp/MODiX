namespace Modix.Data.Models.Core
{
    public class ModixConfig
    {
        public string? DiscordToken { get; set; } = null;

        public string? DbConnection { get; set; } = null;

        public string? LogWebhookToken { get; set; } = null;

        public ulong? LogWebhookId { get; set; } = null;

        public string? DiscordClientId { get; set; } = null;

        public string? DiscordClientSecret { get; set; } = null;

        public int MessageCacheSize { get; set; } = 10;

        public string? ReplUrl { get; set; } = null;

        public string? IlUrl { get; set; } = null;

        public string WebsiteBaseUrl { get; set; } = "https://mod.gg";
    }
}
