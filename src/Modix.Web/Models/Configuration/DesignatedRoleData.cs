using Modix.Data.Models.Core;

namespace Modix.Web.Models.Configuration;

public record DesignatedRoleData(long Id, ulong RoleId, DesignatedRoleType RoleDesignation, string Name);
