using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Discord;

using Modix.Services.Core;

namespace Modix.Services
{
    /// <inheritdoc />
    [Obsolete]
    public abstract class BehaviorBase : IBehavior, IDisposable
    {
        /// <summary>
        /// Constructs a new <see cref="BehaviorBase"/>, with the given dependencies.
        /// </summary>
        /// <param name="serviceProvider">The value to use for <see cref="ServiceProvider"/>.</param>
        public BehaviorBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        ~BehaviorBase()
            => Dispose(false);

        /// <inheritdoc />
        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            await OnStartingAsync();
            IsRunning = true;
        }

        /// <inheritdoc />
        public async Task StopAsync(
            CancellationToken cancellationToken)
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
        /// Executes a given action, asynchronously, within scoped request pipeline.
        /// I.E. the action is executed as if it were an incoming external request,
        /// except that the request is self-authenticated (see <see cref="IAuthorizationService.OnAuthenticatedAsync(ISelfUser)"/>).
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected async Task SelfExecuteRequest(Func<IServiceProvider, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (var serviceScope = ServiceProvider.CreateScope())
            {
                await serviceScope.ServiceProvider.GetRequiredService<IAuthorizationService>()
                    .OnAuthenticatedAsync(serviceScope.ServiceProvider.GetRequiredService<IDiscordClient>()
                        .CurrentUser);

                try
                {
                    await action.Invoke(serviceScope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"An error occurred executing {action.Method.Name} upon {action.Target.GetType().FullName}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Proxy method for <see cref="SelfExecuteRequest(Func{IServiceProvider, Task})"/>,
        /// which performs dependency resolution, rather than requring the action to perform it manually through an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService">A dependency to be resolved and injected into <paramref name="action"/>.</typeparam>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected Task SelfExecuteRequest<TService>(Func<TService, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return SelfExecuteRequest(serviceProvider =>
                action.Invoke(serviceProvider.GetRequiredService<TService>()));
        }

        /// <summary>
        /// Proxy method for <see cref="SelfExecuteRequest(Func{IServiceProvider, Task})"/>,
        /// which performs dependency resolution, rather than requring the action to perform it manually through an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService1">A dependency to be resolved and injected into <paramref name="action"/>.</typeparam>
        /// <typeparam name="TService2">A dependency to be resolved and injected into <paramref name="action"/>.</typeparam>
        /// <param name="action">The action to be executed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        internal protected Task SelfExecuteRequest<TService1, TService2>(Func<TService1, TService2, Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return SelfExecuteRequest(serviceProvider =>
                action.Invoke(
                    serviceProvider.GetRequiredService<TService1>(),
                    serviceProvider.GetRequiredService<TService2>()));
        }

        /// <summary>
        /// An <see cref="IServiceProvider"/> to be used for interacting with and retrieving application services.
        /// </summary>
        internal protected IServiceProvider ServiceProvider { get; }
    }
}
