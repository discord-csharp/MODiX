#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Modix.Bot.Attributes;
using Modix.Services.CommandHelp;
using Modix.Services.Diagnostics;

namespace Modix.Modules
{
    [ModuleHelp("Ping", "Provides commands related to determining connectivity and latency.")]
    public sealed class PingModule : InteractionModuleBase
    {
        public PingModule(
            IEnumerable<IAvailabilityEndpoint> availabilityEndpoints,
            IEnumerable<ILatencyEndpoint> latencyEndpoints)
        {
            _availabilityEndpoints = availabilityEndpoints.ToArray();
            _latencyEndpoints = latencyEndpoints.ToArray();
        }

        [SlashCommand("ping", "Ping MODiX to determine connectivity and latency.")]
        [DoNotDefer]
        public async Task PingAsync()
        {
            await RespondAsync($"Pinging {_latencyEndpoints.Count} latency endpoints " +
                $"and {_availabilityEndpoints.Count} availability endpoints...", allowedMentions: AllowedMentions.None);

            var embed = new EmbedBuilder()
                .WithTitle($"Pong! Checked {_latencyEndpoints.Count + _availabilityEndpoints.Count} endpoints");

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(Timeout);

            var latencyResults = await Task.WhenAll(_latencyEndpoints
                .Select(async x => (x.DisplayName, Latency: await x.GetLatencyAsync(cancellationTokenSource.Token))));

            var availabilityResults = await Task.WhenAll(_availabilityEndpoints
                .Select(async x => (x.DisplayName, IsAvailable: await x.GetAvailabilityAsync(cancellationTokenSource.Token))));

            var average = latencyResults
                .Where(x => x.Latency.HasValue)
                .Select(x => x.Latency.GetValueOrDefault())
                .AverageOrNull();

            embed = embed
                .WithCurrentTimestamp()
                .WithColor(LatencyColor(average));

            foreach (var (displayName, latency) in latencyResults)
                embed.AddField(displayName, FormatLatency(latency, "📶"), true);

            foreach (var (displayName, isAvailable) in availabilityResults)
                embed.AddField(displayName, FormatAvailability(isAvailable), true);

            embed.AddField("Average", FormatLatency(average, "📈"), true);

            await ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed.Build();
                message.Content = "";
            });
        }

        private static string FormatLatency(
            double? latency,
            string icon)
        {
            if (latency == -1)
                return $"{icon} Error ⚠️";

            var suffix = latency switch
            {
                _ when (latency is null)    => "💔",
                _ when (latency > 300)      => "💔",
                _ when (latency > 100)      => "💛", //Yellow heart - trust me
                _ when (latency <= 100)     => "💚", //Green heart - trust me again
                _                           => "❓"
            };

            return $"{icon} {latency: 0}ms {suffix}";
        }

        private static string FormatAvailability(
                bool isAvailable)
            => isAvailable ? "🌐 Up 💚" : "🌐 Down 💔";

        private static Color LatencyColor(
                double? latency)
            => latency switch
            {
                _ when (latency is null)    => Color.Red,
                _ when (latency > 300)      => Color.Red,
                _ when (latency > 100)      => Color.Gold,
                _ when (latency <= 100)     => Color.Green,
                _                           => Color.Default
            };

        private const int Timeout
            = 5000;

        private readonly IReadOnlyList<IAvailabilityEndpoint> _availabilityEndpoints;
        private readonly IReadOnlyList<ILatencyEndpoint> _latencyEndpoints;
    }
}
