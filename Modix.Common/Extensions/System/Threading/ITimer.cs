namespace System.Threading
{
    /// <summary>
    /// Provides a mechanism for executing a method on a thread pool thread at specified intervals.
    /// </summary>
    public interface ITimer : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Changes the start time and the interval between method invocations for a timer, using 32-bit signed integers to measure time intervals.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="ITimer"/> was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately.</param>
        /// <param name="period">The time interval between invocations of the callback method specified when the Timer was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="ITimer"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter is negative and is not equal to <see cref="Timeout.Infinite"/>.</exception>
        /// <returns><see langword="true"/> if the timer was successfully updated; otherwise, <see langword="false"/>.</returns>
        bool Change(int dueTime, int period);

        /// <summary>
        /// Changes the start time and the interval between method invocations for a timer, using 64-bit signed integers to measure time intervals.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="ITimer"/> was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. This value must be less than or equal to 4294967294.</param>
        /// <param name="period">The time interval between invocations of the callback method specified when the Timer was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="ITimer"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> or <paramref name="period"/> is less than -1.
        /// -or-
        /// <paramref name="dueTime"/> or <paramref name="period"/> is greater than 4294967294.</exception>
        /// <returns><see langword="true"/> if the timer was successfully updated; otherwise, <see langword="false"/>.</returns>
        bool Change(long dueTime, long period);

        /// <summary>
        /// Changes the start time and the interval between method invocations for a timer, using <see cref="TimeSpan"/> values to measure time intervals.
        /// </summary>
        /// <param name="dueTime">A <see cref="TimeSpan"/> representing the amount of time to delay before invoking the callback method specified when the <see cref="ITimer"/> was constructed, in milliseconds. Specify negative one (-1) milliseconds to prevent the timer from restarting. Specify zero (0) to restart the timer immediately.</param>
        /// <param name="period">The time interval between invocations of the callback method specified when the Timer was constructed, in milliseconds. Specify negative one (-1) milliseconds to disable periodic signaling.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="ITimer"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter, in milliseconds, is less than -1.</exception>
        /// <exception cref="NotSupportedException">The <paramref name="dueTime"/> or <paramref name="period"/> parameter, in milliseconds, is greater than 4294967294.</exception>
        /// <returns><see langword="true"/> if the timer was successfully updated; otherwise, <see langword="false"/>.</returns>
        bool Change(TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Changes the start time and the interval between method invocations for a timer, using 32-bit unsigned integers to measure time intervals.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="ITimer"/> was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. This value must be less than or equal to 4294967294.</param>
        /// <param name="period">The time interval between invocations of the callback method specified when the Timer was constructed, in milliseconds. Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="ITimer"/> has already been disposed.</exception>
        /// <returns><see langword="true"/> if the timer was successfully updated; otherwise, <see langword="false"/>.</returns>
        bool Change(uint dueTime, uint period);
    }
}
