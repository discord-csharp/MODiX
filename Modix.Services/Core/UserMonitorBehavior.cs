using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Implements a behavior for keeping the data within an <see cref="IUserRepository"/> synchronized with Discord.NET.
    /// </summary>
    public class UserMonitorBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="UserMonitorBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordClient"/>.</exception>
        public UserMonitorBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }

        internal protected override Task OnStartingAsync()
        {
            DiscordClient.UserJoined += OnUserJoinedAsync;
            DiscordClient.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            DiscordClient.MessageReceived += OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        internal protected override Task OnStoppingAsync()
        {
            DiscordClient.UserJoined -= OnUserJoinedAsync;
            DiscordClient.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            DiscordClient.MessageReceived -= OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsStarted)
                OnStoppingAsync();

            base.Dispose(disposeManaged);
        }

        /// <summary>
        /// A <see cref="DiscordSocketClient"/> to be used for interacting with the Discord API.
        /// </summary>
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnUserJoinedAsync(IGuildUser guildUser)
            => ExecuteScopedAsync(serviceProvider => CreateOrUpdateUserAsync(serviceProvider, guildUser));

        private Task OnGuildMemberUpdatedAsync(IGuildUser oldUser, IGuildUser newUser)
            => ExecuteScopedAsync(serviceProvider => CreateOrUpdateUserAsync(serviceProvider, newUser));

        private Task OnMessageReceivedAsync(IMessage message)
            => ExecuteScopedAsync(serviceProvider => CreateOrUpdateUserAsync(serviceProvider, message.Author));

        private async Task CreateOrUpdateUserAsync(IServiceProvider serviceProvider, IUser user)
        {
            var userRepository = ServiceProvider.GetService<IUserRepository>();

            var guildUser = user as IGuildUser;

            var success = await userRepository.UpdateAsync(user.Id, x =>
            {
                x.Username = user.Username;
                x.Discriminator = user.Discriminator;
                x.LastSeen = DateTimeOffset.Now;
                if (guildUser != null)
                {
                    x.Nickname = guildUser.Nickname;
                }
            });

            if (!success)
            {
                await userRepository.CreateAsync(new UserCreationData()
                {
                    Id = user.Id,
                    Username = user.Username,
                    Discriminator = user.Discriminator,
                    Nickname = guildUser?.Nickname,
                    FirstSeen = DateTimeOffset.Now,
                    LastSeen = DateTimeOffset.Now
                });
            }
        }
    }
}
