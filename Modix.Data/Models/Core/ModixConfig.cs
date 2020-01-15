namespace Modix.Data.Models.Core
{
    public class ModixConfig
    {
        public string DiscordToken { get; set; } = null!;

        public string StackoverflowToken { get; set; } = null!;

        public string DbConnection { get; set; } = null!;

        public string? LogWebhookToken { get; set; }

        public ulong LogWebhookId { get; set; }

        public string? DiscordClientId { get; set; }

        public string? DiscordClientSecret { get; set; }

        public int MessageCacheSize { get; set; } = 10;

        public string? ReplUrl { get; set; }

        public string? IlUrl { get; set; }

        public string WebsiteBaseUrl { get; set; } = "https://mod.gg";

        public bool EnableStatsd { get; set; }
    }
}
