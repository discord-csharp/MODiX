using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Authorization
{
    public static class AuthorizationSetup
    {
        public static IServiceCollection AddModixAuthorization(this IServiceCollection services)
            => services.AddScoped<IAuthorizationService, AuthorizationService>();
    }
}
