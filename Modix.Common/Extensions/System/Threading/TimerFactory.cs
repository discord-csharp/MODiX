using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace System.Threading
{
    /// <summary>
    /// Provides methods for construction of `<see cref="ITimer"/> objects.
    /// </summary>
    public interface ITimerFactory
    {
        /// <summary>
        /// Initializes a new instance of an <see cref="ITimer"/> with an infinite period and an infinite due time, using the newly created <see cref="ITimer"/> object as the state object.
        /// </summary>
        /// <param name="callback">A <see cref="TimerCallback"/> delegate representing a method to be executed.</param>
        /// <returns>The newly constructed <see cref="ITimer"/>.</returns>
        ITimer CreateTimer(TimerCallback callback);

        /// <summary>
        /// Initializes a new instance of an <see cref="ITimer"/>, using a 32-bit signed integer to specify the time interval.
        /// </summary>
        /// <param name="callback">A <see cref="TimerCallback"/> delegate representing a method to be executed.</param>
        /// <param name="state">An object containign information to be used by the callback methods, or <see langword="null"/>.</param>
        /// <param name="dueTime">The amount of time to delay before <paramref name="callback"/> is invoked, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from starting. Specify zero (0) to start the timer immediately.</param>
        /// <param name="period">The time interval between invocations of <paramref name="callback"/>, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter is negative and is not equal to <see cref="Timeout.Infinite"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <returns>The newly constructed <see cref="ITimer"/>.</returns>
        ITimer CreateTimer(TimerCallback callback, object? state, int dueTime, int period);

        /// <summary>
        /// Initializes a new instance of an <see cref="ITimer"/>, using 64-bit signed integers to measure time intervals.
        /// </summary>
        /// <param name="callback">A <see cref="TimerCallback"/> delegate representing a method to be executed.</param>
        /// <param name="state">An object containign information to be used by the callback methods, or <see langword="null"/>.</param>
        /// <param name="dueTime">The amount of time to delay before <paramref name="callback"/> is invoked, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from starting. Specify zero (0) to start the timer immediately.</param>
        /// <param name="period">The time interval between invocations of <paramref name="callback"/>, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter is negative and is not equal to <see cref="Timeout.Infinite"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter is negative and is greater than 4294967294.</exception>
        /// <returns>The newly constructed <see cref="ITimer"/>.</returns>
        ITimer CreateTimer(TimerCallback callback, object? state, long dueTime, long period);

        /// <summary>
        /// Initializes a new instance of an <see cref="ITimer"/>, using a <see cref="TimeSpan"/> values to measure time intervals.
        /// </summary>
        /// <param name="callback">A <see cref="TimerCallback"/> delegate representing a method to be executed.</param>
        /// <param name="state">An object containign information to be used by the callback methods, or <see langword="null"/>.</param>
        /// <param name="dueTime">The amount of time to delay before <paramref name="callback"/> is invoked, in milliseconds. Specify negative one (-1) milliseconds to prevent the timer from starting. Specify zero (0) to start the timer immediately.</param>
        /// <param name="period">The time interval between invocations of <paramref name="callback"/>, in milliseconds. Specify negative one (-1) milliseconds to disable periodic signaling.</param>
        /// <exception cref="ArgumentOutOfRangeException">The number of milliseconds in the value of <paramref name="dueTime"/> or <paramref name="period"/> is negative and not equal to <see cref="Timeout.Infinite"/>, or is greater than <see cref="int.MaxValue"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <returns>The newly constructed <see cref="ITimer"/>.</returns>
        ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Initializes a new instance of an <see cref="ITimer"/>, using 32-bit unsigned integers to measure time intervals.
        /// </summary>
        /// <param name="callback">A <see cref="TimerCallback"/> delegate representing a method to be executed.</param>
        /// <param name="state">An object containign information to be used by the callback methods, or <see langword="null"/>.</param>
        /// <param name="dueTime">The amount of time to delay before <paramref name="callback"/> is invoked, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from starting. Specify zero (0) to start the timer immediately.</param>
        /// <param name="period">The time interval between invocations of <paramref name="callback"/>, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter is negative and is not equal to <see cref="Timeout.Infinite"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        /// <returns>The newly constructed <see cref="ITimer"/>.</returns>
        ITimer CreateTimer(TimerCallback callback, object? state, uint dueTime, uint period);
    }

    [ServiceBinding(ServiceLifetime.Singleton)]
    public class TimerFactory
        : ITimerFactory
    {
        /// <inheritdoc/>
        public ITimer CreateTimer(TimerCallback callback)
            => new ProxyTimer(new Timer(callback));

        /// <inheritdoc/>
        public ITimer CreateTimer(TimerCallback callback, object? state, int dueTime, int period)
            => new ProxyTimer(new Timer(callback, state, dueTime, period));

        /// <inheritdoc/>
        public ITimer CreateTimer(TimerCallback callback, object? state, long dueTime, long period)
            => new ProxyTimer(new Timer(callback, state, dueTime, period));

        /// <inheritdoc/>
        public ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            => new ProxyTimer(new Timer(callback, state, dueTime, period));

        /// <inheritdoc/>
        public ITimer CreateTimer(TimerCallback callback, object? state, uint dueTime, uint period)
            => new ProxyTimer(new Timer(callback, state, dueTime, period));

        private class ProxyTimer : ITimer
        {
            public ProxyTimer(Timer timer)
            {
                _timer = timer;
            }

            private readonly Timer _timer;

            /// <inheritdoc/>
            public bool Change(int dueTime, int period)
                => _timer.Change(dueTime, period);

            /// <inheritdoc/>
            public bool Change(long dueTime, long period)
                => _timer.Change(dueTime, period);

            /// <inheritdoc/>
            public bool Change(TimeSpan dueTime, TimeSpan period)
                => _timer.Change(dueTime, period);

            /// <inheritdoc/>
            public bool Change(uint dueTime, uint period)
                => _timer.Change(dueTime, period);

            /// <inheritdoc/>
            public ValueTask DisposeAsync()
                => _timer.DisposeAsync();

            /// <inheritdoc/>
            public void Dispose()
                => _timer.Dispose();
        }
    }
}
