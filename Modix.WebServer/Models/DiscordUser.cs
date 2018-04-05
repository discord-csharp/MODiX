using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
    
namespace Modix.WebServer.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        Member,
        Staff
    }

    public class DiscordUser
    {
        public string Name { get; set; }

        [JsonConverter(typeof(ToStringJsonConverter))]
        public ulong UserId { get; set; }
        public string AvatarHash { get; set; }
        public UserRole UserRole { get; set; } = UserRole.Member;

        public static DiscordUser FromClaimsPrincipal(ClaimsPrincipal user)
        {
            if (user?.Identity?.Name == null) { return null; }

            return new DiscordUser
            {
                Name = user.Identity.Name,
                UserId = ulong.Parse(user.Claims.FirstOrDefault(d => d.Type == ClaimTypes.NameIdentifier).Value),
                AvatarHash = user.Claims.FirstOrDefault(d=>d.Type == "avatarHash").Value
            };
        }
    }
}
