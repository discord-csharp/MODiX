using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;

using Modix.Services.Core;

namespace Modix.Services
{
    /// <inheritdoc />
    public abstract class BehaviorBase : IBehavior, IDisposable
    {
        /// <summary>
        /// Constructs a new <see cref="BehaviorBase"/>, with the given dependencies.
        /// </summary>
        /// <param name="serviceProvider">The value to use for <see cref="ServiceProvider"/>.</param>
        public BehaviorBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        ~BehaviorBase()
            => Dispose(false);

        /// <inheritdoc />
        public async Task StartAsync()
        {
            await OnStartingAsync();
            IsRunning = true;
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            IsRunning = false;
            await OnStoppedAsync();
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

        internal protected bool IsRunning { get; private set; }
            = false;

        /// <summary>
        /// Allows subclasses to inject logic into <see cref="StartAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected abstract Task OnStartingAsync();

        /// <summary>
        /// Allows subclasses to inject logic into <see cref="StopAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected abstract Task OnStoppedAsync();

        /// <summary>
        /// Executes a given action, asynchronously, upon a service, within a new service scope.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected async Task SelfExecuteOnScopedServiceAsync<TService>(Func<TService, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (var serviceScope = ServiceProvider.CreateScope())
            {
                await serviceScope.ServiceProvider.GetRequiredService<IAuthorizationService>()
                    .OnAuthenticatedAsync(serviceScope.ServiceProvider.GetRequiredService<IDiscordClient>()
                        .CurrentUser);

                await action.Invoke(serviceScope.ServiceProvider.GetRequiredService<TService>());
            }
        }

        /// <summary>
        /// An <see cref="IServiceProvider"/> to be used for interacting with and retrieving application services.
        /// </summary>
        internal protected IServiceProvider ServiceProvider { get; }
    }
}
