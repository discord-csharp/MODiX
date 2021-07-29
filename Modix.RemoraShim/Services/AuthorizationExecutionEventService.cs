using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Modix.RemoraShim.Errors;
using Modix.Services.Core;

using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Modix.RemoraShim.Services
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class AuthorizationExecutionEventService : IExecutionEventService
    {
        public AuthorizationExecutionEventService(
            IAuthorizationService authorizationService,
            IDiscordRestChannelAPI channelApi,
            IUserService userService)
        {
            _authorizationService = authorizationService;
            _channelApi = channelApi;
            _userService = userService;
        }

        public Task<Result> AfterExecutionAsync(ICommandContext context, IResult executionResult, CancellationToken ct = default)
            => Task.FromResult(Result.FromSuccess());

        public async Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested || !context.GuildID.HasValue)
                return Result.FromSuccess();

            try
            {
                var channelResult = await _channelApi.GetChannelAsync(context.ChannelID, ct);
                if (!channelResult.IsSuccess)
                    return Result.FromError(channelResult);

                if (channelResult.Entity is { ThreadMetadata: { HasValue: false } })
                    return Result.FromError(new NonThreadContextError(context.ChannelID));

                var guildUser = await _userService.GetGuildUserAsync(context.GuildID.Value.Value, context.User.ID.Value);
                await _authorizationService.OnAuthenticatedAsync(guildUser);
                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(new ExceptionError(ex));
            }
        }

        private readonly IAuthorizationService _authorizationService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IUserService _userService;
    }
}
