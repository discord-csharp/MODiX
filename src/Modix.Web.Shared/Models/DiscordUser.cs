namespace Modix.Web.Shared.Models;

public class DiscordUser
{
    public required ulong UserId { get; init; }
    public required string Name { get; init; }
    public required string AvatarHash { get; init; }
    public required IEnumerable<string> Claims { get; set; }
}
