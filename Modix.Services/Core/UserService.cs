using System;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

using Modix.Services.Core;

namespace Modix.Services.Core
{
    /// <inheritdoc />
    public class UserService : IUserService
    {
        /// <summary>
        /// Constructs a new <see cref="UserService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="authenticationService">The value to use for <see cref="AuthenticationService"/>.</param>
        /// <param name="guildService">The value to use for <see cref="GuildService"/>.</param>
        /// <param name="userRepository">The value to use for <see cref="UserRepository"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for all parameters.</exception>
        public UserService(IDiscordClient discordClient, IAuthenticationService authenticationService, IGuildService guildService, IUserRepository userRepository)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            AuthenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            GuildService = guildService ?? throw new ArgumentNullException(nameof(guildService));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <inheritdoc />
        public Task<IUser> GetCurrentUserAsync()
            => (AuthenticationService.CurrentUserId == null)
                ? Task.FromResult<IUser>(null)
                : GetUserAsync(AuthenticationService.CurrentUserId.Value);

        /// <inheritdoc />
        public async Task<IUser> GetUserAsync(ulong userId)
        {
            var guild = await GuildService.GetCurrentGuildAsync();

            var user = await guild?.GetUserAsync(userId)
                ?? await DiscordClient.GetUserAsync(userId);

            if (user == null)
                throw new InvalidOperationException($"Discord user {userId} does not exist");

            await CreateOrUpdateModixUserAsync(user);

            return user;
        }

        /// <inheritdoc />
        public async Task<IGuildUser> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            var guild = await GuildService.GetGuildAsync(guildId);
            if (guild == null)
                return null;

            var user = await guild.GetUserAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Discord user {userId} does not exist");

            await CreateOrUpdateModixUserAsync(user);

            return user;
        }

        /// <summary>
        /// A <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthenticationService"/> to be used to interact with the current authenticated user.
        /// </summary>
        internal protected IAuthenticationService AuthenticationService { get; }

        /// <summary>
        /// A <see cref="IGuildService"/> to be used to interact with Discord guild objects.
        /// </summary>
        internal protected IGuildService GuildService { get; }

        /// <summary>
        /// A <see cref="IUserRepository"/> to be used to interact with user data within a datastore.
        /// </summary>
        internal protected IUserRepository UserRepository { get; }

        private async Task CreateOrUpdateModixUserAsync(IUser user)
        {
            var guildUser = user as IGuildUser;

            if (await UserRepository.ExistsAsync(user.Id))
            {
                await UserRepository.UpdateAsync(user.Id, data =>
                {
                    data.Username = user.Username;
                    data.Discriminator = user.Discriminator;
                    data.Nickname = guildUser?.Nickname;
                    data.LastSeen = DateTimeOffset.Now;
                });
            }
            else
            {
                await UserRepository.CreateAsync(new UserCreationData()
                {
                    Id = user.Id,
                    Username = user.Username,
                    Discriminator = user.Discriminator,
                    Nickname = guildUser?.Nickname,
                    FirstSeen = DateTimeOffset.Now,
                    LastSeen = DateTimeOffset.Now,
                });
            }
        }
    }
}
