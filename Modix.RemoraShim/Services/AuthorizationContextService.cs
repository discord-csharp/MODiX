using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Core;

using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Services
{
    /// <summary>
    /// Provides the ability to authenticate a user within the current scope.
    /// </summary>
    internal interface IAuthorizationContextService
    {
        Task<Result> SetCurrentAuthenticatedUserAsync(Snowflake guildId, Snowflake userId = default);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class AuthorizationContextService : IAuthorizationContextService
    {
        public AuthorizationContextService(
            IAuthorizationService authorizationService,
            IDiscordRestUserAPI userApi,
            IUserService userService)
        {
            _authorizationService = authorizationService;
            _userApi = userApi;
            _userService = userService;
        }

        public async Task<Result> SetCurrentAuthenticatedUserAsync(Snowflake guildId, Snowflake userId = default)
        {
            try
            {
                if (userId == default)
                {
                    var selfUserResult = await _userApi.GetCurrentUserAsync();
                    if (!selfUserResult.IsSuccess)
                        return Result.FromError(selfUserResult);

                    userId = selfUserResult.Entity.ID;
                }

                var guildUser = await _userService.GetGuildUserAsync(guildId.Value, userId.Value);
                await _authorizationService.OnAuthenticatedAsync(guildUser.Id, guildUser.GuildId, guildUser.RoleIds.ToList());
                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(new ExceptionError(ex));
            }
        }

        private readonly IAuthorizationService _authorizationService;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IUserService _userService;
    }
}
