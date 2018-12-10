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
    public class DiscordWebhookSink : ILogEventSink
    {
        private readonly ulong _webhookId;
        private readonly string _webhookToken;
        private readonly IFormatProvider _formatProvider;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public DiscordWebhookSink(
            ulong webhookId,
            string webhookToken,
            IFormatProvider formatProvider,
            CodePasteService codePasteService)
        {
            _webhookId = webhookId;
            _webhookToken = webhookToken;
            _formatProvider = formatProvider;
            CodePasteService = codePasteService;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new ExceptionContractResolver()
            };
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
                    .WithValue(Format.Code($"{formattedMessage}\n{logEvent.Exception?.Message}")));
            try
            {
                var eventAsJson = JsonConvert.SerializeObject(logEvent, _jsonSerializerSettings);

                var url = CodePasteService.UploadCodeAsync(eventAsJson, "json").GetAwaiter().GetResult();

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Full Log Event")
                    .WithValue($"[view on paste.mod.gg]({url})"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to upload log event.{ex}");
                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Stack Trace")
                    .WithValue(Format.Code($"{formattedMessage}\n{logEvent.Exception?.ToString().TruncateTo(1000)}")));
                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Upload Failure Exception")
                    .WithValue(Format.Code($"{ex.ToString().TruncateTo(1000)}")));
            }
            webhookClient.SendMessageAsync(string.Empty, embeds: new[] { message.Build() }, username: "Modix Logger");
        }

        protected CodePasteService CodePasteService { get; }
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
