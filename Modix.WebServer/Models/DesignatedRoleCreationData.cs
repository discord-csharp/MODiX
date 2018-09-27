using System;
using System.Collections.Generic;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.WebServer.Models
{
    public class DesignatedRoleCreationData
    {
        public ulong RoleId { get; set; }
        public List<DesignatedRoleType> RoleDesignations { get; set; }
    }
}
