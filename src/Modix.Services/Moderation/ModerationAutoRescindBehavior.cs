using System;
using System.Threading.Tasks;
using System.Timers;

using Discord.WebSocket;

using Modix.Data.Repositories;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a behavior that automatically rescinds infractions, as they expire.
    /// </summary>
    public class ModerationAutoRescindBehavior : BehaviorBase, IInfractionEventHandler
    {
        // TODO: Abstract DiscordSocketClient to DiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="ModerationAutoRescindBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        public ModerationAutoRescindBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;

            UpdateTimer = new Timer()
            {
                AutoReset = false,
                Enabled = false,
            };
            UpdateTimer.Elapsed += OnUpdateTimerElapsed;
        }

        /// <inheritdoc />
        public Task OnInfractionCreatedAsync(long infractionId, InfractionCreationData data)
        {
            if (data.Duration is TimeSpan dataDuration
                && (!UpdateTimer.Enabled
                    || (DateTimeOffset.UtcNow + dataDuration) < _nextTick))
            {
                SetNextUpdateTimerTrigger(dataDuration);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.Connected += OnDiscordClientConnected;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.Connected -= OnDiscordClientConnected;

            UpdateTimer.Stop();

            return Task.CompletedTask;
        }

        // TODO: Abstract DiscordSocketClient to DiscordSocketClient, or something, to make this testable
        /// <summary>
        /// A <see cref="DiscordSocketClient"/> for interacting with, and receiving events from, the Discord API.
        /// </summary>
        internal protected DiscordSocketClient DiscordClient { get; }

        /// <summary>
        /// A timer that will fire when an update should be run, to check for and rescind any expired infractions.
        /// </summary>
        internal protected Timer UpdateTimer { get; }

        private Task OnDiscordClientConnected()
        {
            SetNextUpdateTimerTrigger(TimeSpan.Zero);

            return Task.CompletedTask;
        }

        private void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
#pragma warning disable CS4014
            SelfExecuteRequest<ModerationService>(async moderationService =>
            {
                await moderationService.AutoRescindExpiredInfractions();

                var nextExpiration = await moderationService.GetNextInfractionExpiration();

                if (nextExpiration != null)
                    SetNextUpdateTimerTrigger(nextExpiration.Value - DateTimeOffset.UtcNow);
            });
#pragma warning restore CS4014
        }

        private void SetNextUpdateTimerTrigger(TimeSpan interval)
        {
            var newInterval =
                (interval.TotalMilliseconds < MinTimerInterval) ? MinTimerInterval :
                (interval.TotalMilliseconds > MaxTimerInterval) ? MaxTimerInterval :
                interval.TotalMilliseconds;

            _nextTick = DateTimeOffset.UtcNow.AddMilliseconds(newInterval);

            UpdateTimer.Interval = newInterval;

            UpdateTimer.Start();
        }

        private const double MinTimerInterval
            = 1; // Timer doesn't allow for 0

        private const double MaxTimerInterval
            = 3600000; // 1 hour

        private DateTimeOffset _nextTick;
    }
}
