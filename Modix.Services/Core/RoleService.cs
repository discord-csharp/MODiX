using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord roles, within the application.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Updates information about the given role within the role tracking feature.
        /// </summary>
        /// <param name="role">The role whose info is to be tracked.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackRoleAsync(IRole role);
    }

    /// <inheritdoc />
    public class RoleService : IRoleService
    {
        /// <summary>
        /// Constructs a new <see cref="RoleService"/> with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        public RoleService(IDiscordClient discordClient, IGuildRoleRepository guildRoleRepository)
        {
            DiscordClient = discordClient;
            GuildRoleRepository = guildRoleRepository;
        }

        /// <inheritdoc />
        public async Task TrackRoleAsync(IRole role)
        {
            using (var transaction = await GuildRoleRepository.BeginCreateTransactionAsync())
            {
                if (!(await GuildRoleRepository.TryUpdateAsync(role.Id, data =>
                {
                    data.Name = role.Name;
                    data.Position = role.Position;
                })))
                {
                    await GuildRoleRepository.CreateAsync(new GuildRoleCreationData()
                    {
                        RoleId = role.Id,
                        GuildId = role.Guild.Id,
                        Name = role.Name,
                        Position = role.Position
                    });
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IGuildRoleRepository"/> to be used to interact with role data within a datastore.
        /// </summary>
        internal protected IGuildRoleRepository GuildRoleRepository { get; }
    }
}
