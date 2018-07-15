using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services
{
    public abstract class BehaviorBase : IBehavior, IDisposable
    {
        public BehaviorBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        ~BehaviorBase()
            => Dispose(false);

        public async Task StartAsync()
        {
            await OnStartingAsync();
            IsStarted = true;
        }

        public async Task StopAsync()
        {
            await OnStoppingAsync();
            IsStarted = false;
        }

        /// <summary>
        /// See <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public void Dispose()
        {
            if (!_hasDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _hasDisposed = true;
            }
        }

        private bool _hasDisposed
            = false;

        /// <summary>
        /// Allows subclasses to inject logic into <see cref="Dispose"/>.
        /// </summary>
        /// <param name="disposeManaged">A flag indicating whether managed resources should be disposed.</param>
        internal protected virtual void Dispose(bool disposeManaged) { }

        internal protected bool IsStarted { get; private set; }
            = false;

        internal protected abstract Task OnStartingAsync();

        internal protected abstract Task OnStoppingAsync();

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
