using System.Threading.Tasks;

namespace Modix.Services.Authorization
{
    public interface IAuthorizationService
    {
        Task RequireClaimsAsync(params AuthorizationClaim[] claim);
    }
}
