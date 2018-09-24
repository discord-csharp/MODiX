using System;
using System.Collections.Generic;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.Data.Utilities
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ClaimInfoAttribute : Attribute
    {
        public AuthorizationClaimCategory Category { get; set; }
        public string Description { get; set; }

        public ClaimInfoAttribute(AuthorizationClaimCategory category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}
