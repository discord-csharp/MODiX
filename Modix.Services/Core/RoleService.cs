#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    public interface IRoleService
    {
        Task TrackRoleAsync(
            IRole role,
            CancellationToken cancellationToken);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    public class RoleService
        : IRoleService
    {
        public RoleService(
            IGuildRoleRepository guildRoleRepository,
            ILogger<RoleService> logger)
        {
            _guildRoleRepository = guildRoleRepository;
            _logger = logger;
        }

        public async Task TrackRoleAsync(
            IRole role,
            CancellationToken cancellationToken)
        {
            using var logScope = RolesLogMessages.BeginRoleScope(_logger, role.Guild.Id, role.Id);

            RolesLogMessages.RoleTracking(_logger);

            RolesLogMessages.TransactionBeginning(_logger);
            using var transaction = await _guildRoleRepository.BeginCreateTransactionAsync();

            RolesLogMessages.RoleUpdating(_logger);
            var wasUpdateSuccessful = await _guildRoleRepository.TryUpdateAsync(role.Id, data =>
            {
                data.Name = role.Name;
                data.Position = role.Position;
            });

            if (wasUpdateSuccessful)
                RolesLogMessages.RoleUpdated(_logger);
            else
            {
                RolesLogMessages.RoleUpdateFailed(_logger);
                RolesLogMessages.RoleCreating(_logger);
                await _guildRoleRepository.CreateAsync(new GuildRoleCreationData()
                {
                    RoleId = role.Id,
                    GuildId = role.Guild.Id,
                    Name = role.Name,
                    Position = role.Position
                });
                RolesLogMessages.RoleCreated(_logger);
            }

            RolesLogMessages.TransactionCommitting(_logger);
            transaction.Commit();

            RolesLogMessages.RoleTracked(_logger);
        }

        private readonly IGuildRoleRepository _guildRoleRepository;
        private readonly ILogger _logger;
    }
}
