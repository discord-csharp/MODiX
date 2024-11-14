using System.Threading.Tasks;
using Modix.Data.Models.Core;

namespace Modix.Services;

public interface IScopedSession
{
    ulong SelfUserId { get; }
    ulong ExecutingUserId { get; }
    Task<bool> HasClaim(params AuthorizationClaim[] claims);
}
