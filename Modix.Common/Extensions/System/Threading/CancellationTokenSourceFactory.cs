using Microsoft.Extensions.DependencyInjection;

namespace System.Threading
{
    /// <summary>
    /// Describes an object that creates <see cref="ICancellationTokenSource"/> objects.
    /// </summary>
    public interface ICancellationTokenSourceFactory
    {
        /// <summary>
        /// Initializes a new instance of an <see cref="ICancellationTokenSource"/>.
        /// </summary>
        /// <returns>A new instance of an <see cref="ICancellationTokenSource"/>.</returns>
        ICancellationTokenSource Create();

        /// <summary>
        /// Initializes a new instance of an <see cref="ICancellationTokenSource"/> that will be canceled after the specified delay in milliseconds.
        /// </summary>
        /// <param name="millisecondsDelay">The time interval in milliseconds to wait before canceling this <see cref="ICancellationTokenSource"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsDelay"/> is less than -1.</exception>
        /// <returns>A new instance of an <see cref="ICancellationTokenSource"/>.</returns>
        ICancellationTokenSource Create(int millisecondsDelay);

        /// <summary>
        /// Initializes a new instance of an <see cref="ICancellationTokenSource"/> that will be canceled after the specified time span.
        /// </summary>
        /// <param name="delay">The time interval to wait before canceling this <see cref="ICancellationTokenSource"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/>.<see cref="TimeSpan.TotalMilliseconds"/> is less than -1 or greater than <see cref="int.MaxValue"/>.</exception>
        /// <returns>A new instance of an <see cref="ICancellationTokenSource"/>.</returns>
        ICancellationTokenSource Create(TimeSpan delay);
    }

    /// <inheritdoc/>
    [ServiceBinding(ServiceLifetime.Singleton)]
    public class CancellationTokenSourceFactory
        : ICancellationTokenSourceFactory
    {
        /// <inheritdoc/>
        public ICancellationTokenSource Create()
            => new AbstractableCancellationTokenSource();

        /// <inheritdoc/>
        public ICancellationTokenSource Create(int millisecondsDelay)
            => new AbstractableCancellationTokenSource(millisecondsDelay);

        /// <inheritdoc/>
        public ICancellationTokenSource Create(TimeSpan delay)
            => new AbstractableCancellationTokenSource(delay);

        private class AbstractableCancellationTokenSource
            : CancellationTokenSource,
                ICancellationTokenSource
        {
            public AbstractableCancellationTokenSource()
                : base() { }

            public AbstractableCancellationTokenSource(int millisecondsDelay)
                : base(millisecondsDelay) { }

            public AbstractableCancellationTokenSource(TimeSpan delay)
                : base(delay) { }
        }
    }
}
