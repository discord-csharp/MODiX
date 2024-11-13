using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Modix.Services.Utilities;

public sealed class DiscordWebhookSink : ILogEventSink, IAsyncDisposable
{
    private readonly Lazy<PasteService> _codePasteService;
    private readonly DiscordWebhookClient _discordWebhookClient;
    private readonly IFormatProvider _formatProvider;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _logEventProcessorTask;
    private readonly BlockingCollection<LogEvent> _logEventQueue;

    public DiscordWebhookSink(
        ulong webhookId,
        string webhookToken,
        IFormatProvider formatProvider,
        Lazy<PasteService> codePasteService)
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

        _cancellationTokenSource = new CancellationTokenSource();
        _logEventQueue = [];
        _logEventProcessorTask = Task.Run(ProcessLogEventItemsAsync, _cancellationTokenSource.Token);
    }

    public void Emit(LogEvent logEvent)
        => _logEventQueue.Add(logEvent);

    public async Task ProcessLogEventItemsAsync()
    {
        foreach (var logEvent in _logEventQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
        {
            try
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

                    var url = await _codePasteService.Value.UploadPaste(eventAsJson);

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        message.AddField(new EmbedFieldBuilder()
                            .WithIsInline(false)
                            .WithName("Full Log Event")
                            .WithValue($"[view on paste.mod.gg]({url})"));
                    }
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

                await _discordWebhookClient.SendMessageAsync(string.Empty, embeds: [message.Build()], username: "Modix Logger");
            }
            catch
            {
                // Catching all exceptions as to not crash the processor thread
                // Wait an arbitrary amount of time before trying to process the next item.
                await Task.Delay(10000);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _discordWebhookClient.Dispose();
        await _cancellationTokenSource.CancelAsync();
        await _logEventProcessorTask;
        _cancellationTokenSource.Dispose();
    }
}

public static class DiscordWebhookSinkExtensions
{
    public static LoggerConfiguration DiscordWebhookSink(this LoggerSinkConfiguration config, ulong id, string token, LogEventLevel minLevel, Lazy<PasteService> codePasteService)
        => config.Sink(new DiscordWebhookSink(id, token, null, codePasteService), minLevel);
}

public static class LoggingExtensions
{
    public static string TruncateTo(this string str, int length)
        => str.Length < length
            ? str
            : str[..length];
}
