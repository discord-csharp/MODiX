using System;
using Discord;
using Discord.Webhook;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Modix.Services.Utilities
{
    public class DiscordWebhookSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly ulong _webhookId;
        private readonly string _webhookToken;

        public DiscordWebhookSink(ulong webhookId, string webhookToken, IFormatProvider formatProvider)
        {
            _webhookId = webhookId;
            _webhookToken = webhookToken;
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var formattedMessage = logEvent.RenderMessage(_formatProvider);
            var webhookClient = new DiscordWebhookClient(_webhookId, _webhookToken);

            var message = new EmbedBuilder()
                .WithAuthor("DiscordLogger")
                .WithTitle("Modix")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithColor(Color.Red)
                .AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"LogLevel: {logEvent.Level}")
                    .WithValue(Format.Code($"{formattedMessage}\n{logEvent.Exception}".TruncateTo(1010))));

            webhookClient.SendMessageAsync(string.Empty, embeds: new[] {message.Build()}, username: "Modix Logger");
        }
    }

    public static class DiscordWebhookSinkExtensions
    {
        public static LoggerConfiguration DiscordWebhookSink(this LoggerSinkConfiguration config, ulong id,
            string token, LogEventLevel minLevel)
        {
            return config.Sink(new DiscordWebhookSink(id, token, null), minLevel);
        }
    }

    public static class LoggingExtensions
    {
        public static string TruncateTo(this string str, int length)
        {
            if (str.Length < length) return str;

            return str.Substring(0, length);
        }
    }
}