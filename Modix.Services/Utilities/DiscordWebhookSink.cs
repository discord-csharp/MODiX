using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using Discord.Webhook;
using Discord;
using Modix.Services.CodePaste;
using Newtonsoft.Json;

namespace Modix.Services.Utilities
{
    public sealed class DiscordWebhookSink
        : ILogEventSink,
            IDisposable
    {
        private readonly CodePasteService _codePasteService;
        private readonly DiscordWebhookClient _discordWebhookClient;
        private readonly IFormatProvider _formatProvider;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public DiscordWebhookSink(
            ulong webhookId,
            string webhookToken,
            IFormatProvider formatProvider,
            CodePasteService codePasteService)
        {
            _codePasteService = codePasteService;
            _discordWebhookClient = new DiscordWebhookClient(webhookId, webhookToken);
            _formatProvider = formatProvider;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new ExceptionContractResolver()
            };
        }
        public void Emit(LogEvent logEvent)
        {
            const int DiscordStringTruncateLength = 1000;

            var formattedMessage = logEvent.RenderMessage(_formatProvider);

            var message = new EmbedBuilder()
                    .WithAuthor("DiscordLogger")
                    .WithTitle("Modix")
                    .WithTimestamp(DateTimeOffset.UtcNow)
                    .WithColor(Color.Red);

            try
            {
                var messagePayload = $"{formattedMessage}\n{logEvent.Exception?.Message}";

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"LogLevel: {logEvent.Level}")
                    .WithValue(Format.Code(messagePayload.TruncateTo(DiscordStringTruncateLength))));

                var eventAsJson = JsonConvert.SerializeObject(logEvent, _jsonSerializerSettings);

                var url = _codePasteService.UploadCodeAsync(eventAsJson, "json").GetAwaiter().GetResult();

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Full Log Event")
                    .WithValue($"[view on paste.mod.gg]({url})"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to upload log event. {ex}");

                var stackTracePayload = $"{formattedMessage}\n{logEvent.Exception?.ToString().TruncateTo(DiscordStringTruncateLength)}".TruncateTo(DiscordStringTruncateLength);

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Stack Trace")
                    .WithValue(Format.Code(stackTracePayload)));

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Upload Failure Exception")
                    .WithValue(Format.Code($"{ex.ToString().TruncateTo(DiscordStringTruncateLength)}")));
            }
            _discordWebhookClient.SendMessageAsync(string.Empty, embeds: new[] { message.Build() }, username: "Modix Logger");
        }

        public void Dispose()
            => _discordWebhookClient.Dispose();
    }

    public static class DiscordWebhookSinkExtensions
    {
        public static LoggerConfiguration DiscordWebhookSink(this LoggerSinkConfiguration config, ulong id, string token, LogEventLevel minLevel, CodePasteService codePasteService)
        {
            return config.Sink(new DiscordWebhookSink(id, token, null, codePasteService), minLevel);
        }
    }

    public static class LoggingExtensions
    {
        public static string TruncateTo(this string str, int length)
        {
            if (str.Length < length)
            {
                return str;
            }

            return str.Substring(0, length);
        }
    }
}
