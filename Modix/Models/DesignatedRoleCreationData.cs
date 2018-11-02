using System.Collections.Generic;
using Modix.Data.Models.Core;

namespace Modix.Models
{
    public class DesignatedRoleCreationData
    {
        public ulong RoleId { get; set; }
        public List<DesignatedRoleType> RoleDesignations { get; set; }
    }
}
