using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Services;

namespace Modix.Bot;

public class DiscordBotSession(DiscordSocketClient discordSocketClient,
    AuthorizationClaimService authorizationClaimService) : IScopedSession
{
    public ulong SelfUserId { get; } = discordSocketClient.CurrentUser.Id;

    private ulong _executingUserId;

    public ulong ExecutingUserId =>
        _executingUserId == default
            ? SelfUserId
            : _executingUserId;

    private IReadOnlyCollection<AuthorizationClaim> _authorizationClaims;

    public void ApplyCommandContext(ICommandContext context)
    {
        _executingUserId = context.User.Id;
    }

    private async Task<IReadOnlyCollection<AuthorizationClaim>> GetClaims()
    {
        return _authorizationClaims ??= await authorizationClaimService.GetClaimsForUser(ExecutingUserId);
    }

    public async Task<bool> HasClaim(params AuthorizationClaim[] claims)
    {
        var ownedClaims = await GetClaims();
        return claims.All(claim => ownedClaims.Contains(claim));
    }
}
