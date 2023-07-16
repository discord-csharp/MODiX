#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Checks if a user exists that is associated with the given Discord ID value that also belongs to a specified guild.
        /// </summary>
        /// <param name="guildId">The <see cref="IEntity{T}.Id" /> of the guild to be checked for the user.</param>
        /// <param name="userId">The <see cref="IEntity{T}.Id" /> of the user to look for.</param>
        /// <returns>True if the user exists, false if either the guild or user don't exist</returns>
        Task<bool> GuildUserExistsAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Retrieves the user, if any, associated with the given Discord ID value that also belongs to a specified guild.
        /// </summary>
        /// <param name="guildId">The <see cref="IEntity{T}.Id" /> of the guild whose user is to be retrieved.</param>
        /// <param name="userId">The <see cref="IEntity{T}.Id" /> of the user to be retrieved.</param>
        /// <returns>The <see cref="IGuildUser"/>, if any, retrieved from Discord.NET.</returns>
        Task<IGuildUser> GetGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Retrieves the user, if any, associated with the provided guild and user ID.
        /// </summary>
        /// <param name="guild">The guild in which to search for the user.</param>
        /// <param name="userId">The unique Discord snowflake ID of the user to search for.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The <see cref="IGuildUser"/>, if any, that was retrieved from Discord.NET.</returns>
        Task<IGuildUser?> TryGetGuildUserAsync(IGuild guild, ulong userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the summary data for the user with the given user ID, within the given guild
        /// </summary>
        /// <param name="guildId">The <see cref="IEntity{T}.Id" /> of the guild whose user is to be retrieved.</param>
        /// <param name="userId">The <see cref="IEntity{T}.Id" /> of the user to be retrieved.</param>
        /// <returns>The <see cref="GuildUserSummary"/> retrieved</returns>
        Task<GuildUserSummary?> GetGuildUserSummaryAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Retrieves all available information on a user matching the supplied criteria.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild in which the user is being searched.</param>
        /// <param name="userId">The Discord snowflake ID of the user that is being searched for.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the operation completes,
        /// containing all user information that was found for the user.
        /// </returns>
        Task<EphemeralUser?> GetUserInformationAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Retrieves the user, if any, associated with the given user ID.
        /// </summary>
        /// <param name="userId">The Discord snowflake ID of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the operation completes,
        /// containing the user identified by the supplied ID.
        /// Returns <see langword="null"/> if no such user was found.
        /// </returns>
        Task<IUser?> GetUserAsync(ulong userId);

        /// <summary>
        /// Updates information about the given user within the user tracking system of a guild.
        /// </summary>
        /// <param name="user">The user whose info is to be tracked.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackUserAsync(
            IGuildUser user,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates information about the given user within the user tracking system of a guild.
        /// </summary>
        /// <param name="guild">The guild for which user info is to be tracked.</param>
        /// <param name="userId">The <see cref="IEntity{ulong}.Id"/> value of the user whose info is to be tracked.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackUserAsync(IGuild guild, ulong userId);
    }

    /// <inheritdoc />
    public class UserService : IUserService
    {
        /// <summary>
        /// Constructs a new <see cref="UserService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="authorizationService">The value to use for <see cref="AuthorizationService"/>.</param>
        /// <param name="guildUserRepository">The value to use for <see cref="GuildUserRepository"/>.</param>
        public UserService(
            IDiscordClient discordClient,
            DiscordRestClient discordRestClient,
            IAuthorizationService authorizationService,
            IGuildUserRepository guildUserRepository,
            ISystemClock systemClock)
        {
            DiscordClient = discordClient;
            DiscordRestClient = discordRestClient;
            AuthorizationService = authorizationService;
            GuildUserRepository = guildUserRepository;
            _systemClock = systemClock;
        }

        /// <inheritdoc />
        public async Task<bool> GuildUserExistsAsync(ulong guildId, ulong userId)
        {
            var guild = await DiscordClient.GetGuildAsync(guildId);
            if (guild == null)
            { return false; }

            var user = await guild.GetUserAsync(userId);
            if (user == null)
            { return false; }

            await TrackUserAsync(user, default);

            return true;
        }

        /// <inheritdoc />
        public async Task<IGuildUser> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            var guild = await DiscordClient.GetGuildAsync(guildId);
            if (guild == null)
                throw new InvalidOperationException($"Discord guild {guildId} does not exist");

            var user = await guild.GetUserAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Discord user {userId} does not exist");

            await TrackUserAsync(user, default);

            return user;
        }

        public async Task<IGuildUser?> TryGetGuildUserAsync(IGuild guild, ulong userId, CancellationToken cancellationToken)
        {
            var user = await guild.GetUserAsync(userId);

            if (user is not null)
            {
                await TrackUserAsync(user, cancellationToken);
                return user;
            }

            var restUser = await DiscordRestClient.GetUserAsync(userId);

            // Even if they aren't in the guild, we still want to track them.
            if (restUser is not null)
            {
                var ephemeralUser = new EphemeralUser()
                    .WithIUserData(restUser)
                    .WithGuildContext(guild);

                await TrackUserAsync(ephemeralUser, cancellationToken);
                return null;
            }

            throw new InvalidOperationException($"Discord user {userId} does not exist");
        }

        public async Task<GuildUserSummary?> GetGuildUserSummaryAsync(ulong guildId, ulong userId)
        {
            var found = await GuildUserRepository.ReadSummaryAsync(userId, guildId);
            return found;
        }

        /// <inheritdoc />
        public async Task<EphemeralUser?> GetUserInformationAsync(ulong guildId, ulong userId)
        {
            if (userId == 0)
                return null;

            var guildTask = DiscordClient.GetGuildAsync(guildId);
            var userTask = DiscordClient.GetUserAsync(userId);
            var restUserTask = DiscordRestClient.GetUserAsync(userId);
            var guildUserSummary = await GetGuildUserSummaryAsync(guildId, userId);

            var guild = await guildTask;

            var guildUser = await guild.GetUserAsync(userId);
            var ban = await guild.GetBanAsync(userId);

            var user = await userTask;
            var restUser = await restUserTask;

            if (guildUser is { })
                await TrackUserAsync(guildUser, default);

            var buildUser = new EphemeralUser()
                .WithGuildUserSummaryData(guildUserSummary)
                .WithIUserData(restUser)
                .WithIUserData(user)
                .WithIGuildUserData(guildUser)
                .WithGuildContext(guild)
                .WithBanData(ban);

            return buildUser.Id == 0 ? null : buildUser;
        }

        public async Task<IUser?> GetUserAsync(ulong userId)
        {
            // Special-case 0, because Discord.Net throws an exception otherwise.
            if (userId == 0)
            {
                return null;
            }

            var socketUser = await DiscordClient.GetUserAsync(userId);
            if (socketUser is { })
            {
                return socketUser;
            }

            return await DiscordRestClient.GetUserAsync(userId);
        }

        /// <inheritdoc />
        public async Task TrackUserAsync(
            IGuildUser user,
            CancellationToken cancellationToken)
        {
            var now = _systemClock.UtcNow;

            using (var transaction = await GuildUserRepository.BeginCreateTransactionAsync(cancellationToken))
            {
                if (!await GuildUserRepository.TryUpdateAsync(user.Id, user.GuildId, data =>
                {
                    // Only update properties that we were given. Updates can be triggered from several different sources, not all of which have all the user's info.
                    if (user.Username != null)
                        data.Username = user.Username;
                    data.Discriminator = user.Discriminator;
                    if (user.Username != null)
                        data.Nickname = user.Nickname;
                    data.LastSeen = now;
                }, cancellationToken))
                {
                    await GuildUserRepository.CreateAsync(new GuildUserCreationData()
                    {
                        UserId = user.Id,
                        GuildId = user.GuildId,
                        Username = user.Username ?? "[UNKNOWN USERNAME]",
                        Discriminator = user.Discriminator,
                        Nickname = user.Nickname,
                        FirstSeen = now,
                        LastSeen = now
                    }, cancellationToken);
                }

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task TrackUserAsync(IGuild guild, ulong userId)
            => await TrackUserAsync(
                    await guild.GetUserAsync(userId),
                    default);

        /// <summary>
        /// A <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="DiscordRestClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected DiscordRestClient DiscordRestClient { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// A <see cref="IGuildUserRepository"/> to be used to interact with user data within a datastore.
        /// </summary>
        internal protected IGuildUserRepository GuildUserRepository { get; }

        private readonly ISystemClock _systemClock;
    }
}
