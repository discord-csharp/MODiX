using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Modix.Web.Shared;

public partial class MainLayout : LayoutComponentBase
{
    //TODO: Move all of this into NavBar?
    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

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
    }
}
