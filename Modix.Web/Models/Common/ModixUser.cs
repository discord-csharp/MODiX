using Discord;
using Modix.Services.Utilities;

namespace Modix.Web.Models.Common;

public class ModixUser : IAutoCompleteItem
{
    public string Name { get; set; }
    public ulong UserId { get; set; }
    public string AvatarUrl { get; set; }

    public static ModixUser FromIGuildUser(IGuildUser user)
    {
        return new()
        {
            Name = user.GetDisplayName(),
            UserId = user.Id,
            AvatarUrl = user.GetDisplayAvatarUrl() ?? user.GetDefaultAvatarUrl()
        };
    }

    public static ModixUser FromNonGuildUser(IUser user)
    {
        return new()
        {
            Name = user.GetDisplayName(),
            UserId = user.Id,
            AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
        };
    }
}
