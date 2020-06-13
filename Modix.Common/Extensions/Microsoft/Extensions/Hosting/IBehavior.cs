using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Describes an application component that performs actions during startup and shutdown of the application host.
    /// This might sound superfluous compared to <see cref="IHostedService"/>, but the difference is that <see cref="IBehavior"/>
    /// actions are executed in parallel, rather than sequentially. This allows behaviors to have dependencies regarding order of completion,
    /// which can be resolved by having one behavior wait upon another to complete, without resulting in deadlock.
    ///
    /// Note that <see cref="IBehavior"/> implementations must not be registered as scoped.
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Performs startup actions for the behavior.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used by the consumer to cancel the operation, before it completes.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation completes.</returns>
        Task StartAsync(
            CancellationToken cancellationToken);

        /// <summary>
        /// Performs shutdown actions for the behavior.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used by the consumer to cancel the operation, before it completes.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation completes.</returns>
        Task StopAsync(
            CancellationToken cancellationToken);
    }
}
