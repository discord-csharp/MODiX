using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Authentication
{
    public static class AuthenticationSetup
    {
        public static IServiceCollection AddModixAuthentication(this IServiceCollection services)
            => services
                .AddScoped<IAuthenticationService, AuthenticationService>();
    }
}
