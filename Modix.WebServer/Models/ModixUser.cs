using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
    
namespace Modix.WebServer.Models
{
    public class ModixUser
    {
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public string AvatarHash { get; set; }
        public IEnumerable<string> Claims { get; set; }

        public static ModixUser FromClaimsPrincipal(ClaimsPrincipal user)
        {
            if (user?.Identity?.Name == null) { return null; }

            return new ModixUser
            {
                Name = user.Identity.Name,
                UserId = ulong.Parse(user.Claims.FirstOrDefault(d => d.Type == ClaimTypes.NameIdentifier).Value),
                AvatarHash = user.Claims.FirstOrDefault(d=>d.Type == "avatarHash")?.Value ?? "",
                Claims = user.Claims.Where(d=>d.Type == ClaimTypes.Role).Select(d=>d.Value)
            };
        }
    }
}
