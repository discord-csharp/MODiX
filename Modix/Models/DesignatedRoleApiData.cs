using Modix.Data.Models.Core;

namespace Modix.Models
{
    public class DesignatedRoleApiData
    {
        public long Id { get; set; }
        public ulong RoleId { get; set; }
        public DesignatedRoleType RoleDesignation { get; set; }
        public string Name { get; set; }
    }
}
