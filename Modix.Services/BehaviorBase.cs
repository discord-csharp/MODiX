using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services
{
    public abstract class BehaviorBase
    {
        public BehaviorBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        internal protected async Task ExecuteScopedAsync(Func<IServiceProvider, Task> action)
        {
            using (var serviceScope = ServiceProvider.CreateScope())
            {
                await action.Invoke(serviceScope.ServiceProvider);
            }
        }

        internal protected IServiceProvider ServiceProvider { get; }
    }
}
