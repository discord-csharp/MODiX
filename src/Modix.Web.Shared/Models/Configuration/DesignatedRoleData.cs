using Modix.Models.Core;

namespace Modix.Web.Shared.Models.Configuration;

public record DesignatedRoleData(long Id, ulong RoleId, DesignatedRoleType RoleDesignation, string Name);
