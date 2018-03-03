using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using Discord.Webhook;
using Discord;

namespace Modix.Utilities
{
    public class DiscordWebhookSink : ILogEventSink
    {
        private ulong _webhookId;
        private string _webhookToken;
        private IFormatProvider _formatProvider;

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
                    .WithValue(Format.Code($"{formattedMessage}\n{logEvent.Exception?.ToString()}".TruncateTo(1010))));

            webhookClient.SendMessageAsync(string.Empty, embeds: new[] { message.Build() }, username: "Modix Logger");
        }
    }
    public static class DiscordWebhookSinkExtensions
    {
        public static LoggerConfiguration DiscordWebhookSink(this LoggerSinkConfiguration config, ulong id, string token, LogEventLevel minLevel)
        {
            return config.Sink(new DiscordWebhookSink(id, token, null), minLevel);
        }
    }

    public static class LoggingExtensions
    {
        public static string TruncateTo(this string str, int length)
        {
            if(str.Length < length)
            {
                return str;
            }

            return str.Substring(0, length);
        }
    }
}
