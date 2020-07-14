namespace System.Threading
{
    /// <summary>
    /// Describes an object that controls the state of a <see cref="CancellationToken"/>.
    /// </summary>
    public interface ICancellationTokenSource
        : IDisposable
    {
        /// <summary>
        /// Gets whether cancellation has been requested for this <see cref="ICancellationTokenSource"/>.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> associated with this <see cref="ICancellationTokenSource"/>.
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="ICancellationTokenSource"/> has been disposed.</exception>
        /// <exception cref="AggregateException">An aggregate exception containing all exceptions thrown by the registered callbacks on the associates <see cref="CancellationToken"/>.</exception>
        void Cancel();

        /// <summary>
        /// Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed if an exception occurs.
        /// </summary>
        /// <param name="throwOnFirstException"><see langword="true"/> if exceptions should immediately propagate; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ObjectDisposedException">This <see cref="ICancellationTokenSource"/> has been disposed.</exception>
        /// <exception cref="AggregateException">An aggregate exception containing all exceptions thrown by the registered callbacks on the associates <see cref="CancellationToken"/>.</exception>
        void Cancel(bool throwOnFirstException);

        /// <summary>
        /// Schedules a cancel operation on this <see cref="ICancellationTokenSource"/> after the specified number of milliseconds.
        /// </summary>
        /// <param name="millisecondsDelay"></param>
        /// <exception cref="ObjectDisposedException">The exception thrown when this <see cref="ICancellationTokenSource"/> has been disposed.</exception>
        /// <exception cref="AggregateException">The exception thrown when <paramref name="millisecondsDelay"/> is less than -1.</exception>
        void CancelAfter(int millisecondsDelay);

        /// <summary>
        /// Schedules a cancel operation on this <see cref="ICancellationTokenSource"/> after the specified time span.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The exception thrown when this <see cref="ICancellationTokenSource"/> has been disposed.</exception>
        /// <exception cref="AggregateException">The exception thrown when <paramref name="delay"/> is less than -1 or greater than <see cref="int.MaxValue"/>.</exception>
        void CancelAfter(TimeSpan delay);
    }
}
