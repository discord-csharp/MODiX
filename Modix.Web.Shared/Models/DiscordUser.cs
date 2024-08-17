using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Web.Shared.Models;

public class DiscordUser
{
    public required ulong UserId { get; init; }
    public required string Name { get; init; }
    public required string AvatarHash { get; init; }
}
