using Discord;
using Modix.Services.Utilities;

namespace Modix.Web.Models.Common;

public sealed class ModixUser : IAutoCompleteItem
{
    public string? Name { get; init; }
    public ulong UserId { get; init; }
    public string? AvatarUrl { get; init; }

    public static ModixUser FromIGuildUser(IGuildUser user) => new()
    {
        Name = user.GetDisplayName(),
        UserId = user.Id,
        AvatarUrl = user.GetDisplayAvatarUrl() ?? user.GetDefaultAvatarUrl()
    };

    public static ModixUser FromNonGuildUser(IUser user) => new()
    {
        Name = user.GetDisplayName(),
        UserId = user.Id,
        AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
    };
}
