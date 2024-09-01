using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Modix.Web.Shared.Models;

namespace Modix.Web.Wasm.Security;

public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> DefaultUnauthenticatedTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = DefaultUnauthenticatedTask;

    public PersistentAuthenticationStateProvider(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<DiscordUser>(nameof(DiscordUser), out var userInfo) || userInfo is null)
            return;

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim(nameof(DiscordUser.AvatarHash), userInfo.AvatarHash),
            new Claim(ClaimTypes.PostalCode, userInfo.CurrentGuild.ToString())
        ];

        var roles = userInfo.Claims.Select(role => new Claim(ClaimTypes.Role, role));

        var claimsIdentity = new ClaimsIdentity([..claims, ..roles], authenticationType: nameof(PersistentAuthenticationStateProvider));
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authState = new AuthenticationState(claimsPrincipal);
        _authenticationStateTask = Task.FromResult(authState);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}
