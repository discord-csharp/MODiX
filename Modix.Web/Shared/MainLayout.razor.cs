using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Modix.Services.Core;
using Modix.Web.Services;

namespace Modix.Web.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public IAuthorizationService AuthorizationService { get; set; } = null!;

    [Inject]
    public DiscordUserService DiscordUserService { get; set; } = null!;

    public string? AvatarUrl { get; private set; }
    public string? Username { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? false)
            return;

        var avatarHash = authState.User.Claims.FirstOrDefault(x => x.Type == DiscordAuthenticationConstants.Claims.AvatarHash)?.Value;
        var userId = authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        AvatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png";
        Username = authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if (!ulong.TryParse(userId, out var userSnowflake))
            return;

        var currentGuild = DiscordUserService.GetUserGuild();
        var currentUser = currentGuild.GetUser(userSnowflake);

        await AuthorizationService.OnAuthenticatedAsync(currentUser.Id, currentGuild.Id, currentUser.Roles.Select(x => x.Id).ToList());
    }
}
