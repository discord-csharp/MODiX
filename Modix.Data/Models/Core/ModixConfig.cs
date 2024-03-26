﻿namespace Modix.Data.Models.Core
{
    public class ModixConfig
    {
        public string? SeqEndpoint { get; set; } = null;

        public string? SeqKey { get; set; } = null;

        public string? DiscordToken { get; set; } = null;

        public string? StackoverflowToken { get; set; } = null;

        public string? DbConnection { get; set; } = null;

        public string? LogWebhookToken { get; set; } = null;

        public ulong? LogWebhookId { get; set; } = null;

        public string? DiscordClientId { get; set; } = null;

        public string? DiscordClientSecret { get; set; } = null;

        public int MessageCacheSize { get; set; } = 10;

        public string? ReplUrl { get; set; } = null;

        public string? IlUrl { get; set; } = null;

        public string WebsiteBaseUrl { get; set; } = "https://mod.gg";

        public bool EnableStatsd { get; set; }
        public bool UseBlazor { get; set; }
    }
}
