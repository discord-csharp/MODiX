using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.WebServer.Models
{
    public class ClaimInfoData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public AuthorizationClaimCategory Category { get; set; }

        private static Dictionary<AuthorizationClaim, ClaimInfoData> _cachedClaimData;

        public static Dictionary<AuthorizationClaim, ClaimInfoData> GetClaims()
        {
            if (_cachedClaimData == null)
            {
                _cachedClaimData = typeof(AuthorizationClaim).GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary
                (
                    d => (AuthorizationClaim)d.GetValue(null),
                    d =>
                    {
                        var claimInfo = (ClaimInfoAttribute)d.GetCustomAttributes(typeof(ClaimInfoAttribute), true).FirstOrDefault();

                        return new ClaimInfoData
                        {
                            Name = d.Name,
                            Description = claimInfo.Description,
                            Category = claimInfo.Category
                        };
                    }
                );
            }

            return _cachedClaimData;
        }
    }
}
