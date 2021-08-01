using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Gateway.Extensions;

namespace Modix.RemoraShim.Behaviors
{
    public static class Setup
    {
        public static IServiceCollection AddResponders(this IServiceCollection services) {

            var responderTypes = typeof(Setup).Assembly
                .GetExportedTypes()
                .Where(t => t.IsResponder());

            foreach (var responderType in responderTypes)
            {
                services.AddResponder(responderType);
            }

            return services;
        }
    }
}
