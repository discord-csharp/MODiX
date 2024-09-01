using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Modix.Web.Shared.Models;

namespace Modix.Web.Security;

public class PersistingAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private Task<AuthenticationState>? _authenticationStateTask;

    public PersistingAuthenticationStateProvider(PersistentComponentState persistentComponentState)
    {
        _state = persistentComponentState;
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveAuto);

        AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if(_authenticationStateTask is null)
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");

        var authState = await _authenticationStateTask;
        var user = authState.User;
        if (user.Identity?.IsAuthenticated is not true)
            return;

        var userId = user.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var avatarHash = user.FindFirst(DiscordAuthenticationConstants.Claims.AvatarHash);
        if (avatarHash is null || user.Identity.Name is null || !ulong.TryParse(userId, out var userSnowflake))
            return;

        _state.PersistAsJson(nameof(DiscordUser), new DiscordUser
        {
            UserId = userSnowflake,
            Name = user.Identity.Name,
            AvatarHash = avatarHash.Value,
            CurrentGuild = ulong.Parse(user.FindFirstValue(ClaimTypes.PostalCode) ?? ""),
            Claims = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value)
        });
    }

    public void Dispose()
    {
        _authenticationStateTask?.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        _subscription.Dispose();
    }
}
