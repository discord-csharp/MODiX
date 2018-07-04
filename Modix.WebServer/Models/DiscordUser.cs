using System.Linq;
using System.Security.Claims;

namespace Modix.WebServer.Models
{
    public enum UserRole
    {
        Member,
        Staff
    }

    public class DiscordUser
    {
        public string Name { get; set; }

        public ulong UserId { get; set; }
        public string AvatarHash { get; set; }
        public UserRole UserRole { get; set; } = UserRole.Member;

        public static DiscordUser FromClaimsPrincipal(ClaimsPrincipal user)
        {
            if (user?.Identity?.Name == null) return null;

            return new DiscordUser
            {
                Name = user.Identity.Name,
                UserId = ulong.Parse(user.Claims.FirstOrDefault(d => d.Type == ClaimTypes.NameIdentifier).Value),
                AvatarHash = user.Claims.FirstOrDefault(d => d.Type == "avatarHash")?.Value ?? ""
            };
        }
    }
}