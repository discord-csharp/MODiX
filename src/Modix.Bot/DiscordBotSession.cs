using System;
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
    private ulong _executingUserId;

    public ulong ExecutingGuildId { get; private set; }
    public ulong SelfUserId => discordSocketClient.CurrentUser.Id;

    public ulong ExecutingUserId =>
        _executingUserId == default
            ? SelfUserId
            : _executingUserId;

    private IReadOnlyCollection<AuthorizationClaim> _authorizationClaims;

    public void ApplyCommandContext(ICommandContext context)
    {
        _executingUserId = context.User.Id;
        ExecutingGuildId = context.Guild.Id;
    }

    private async Task<IReadOnlyCollection<AuthorizationClaim>> GetClaims()
    {
        if (ExecutingUserId == SelfUserId)
        {
            return Enum.GetValues<AuthorizationClaim>();
        }

        var guild = discordSocketClient.GetGuild(ExecutingGuildId);
        var guildUser = guild.GetUser(ExecutingUserId);

        if (guildUser.GuildPermissions.Administrator)
        {
            return Enum.GetValues<AuthorizationClaim>();
        }

        return _authorizationClaims ??= await authorizationClaimService.GetClaimsForUser(ExecutingGuildId, ExecutingUserId);
    }

    public async Task<bool> HasClaim(params AuthorizationClaim[] claims)
    {
        var ownedClaims = await GetClaims();
        return claims.All(claim => ownedClaims.Contains(claim));
    }
}
