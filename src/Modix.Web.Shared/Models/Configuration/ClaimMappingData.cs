using Modix.Models.Core;

namespace Modix.Web.Shared.Models.Configuration;

public record class ClaimMappingData(ulong? RoleId, AuthorizationClaim Claim, ClaimMappingType Type);
