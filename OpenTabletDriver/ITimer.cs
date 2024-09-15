using System;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A high precision timer.
    /// </summary>
    [PublicAPI]
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Starts the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the timer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Current state of the timer.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The timer's interval for invoking <see cref="Elapsed"/> in milliseconds.
        /// </summary>
        float Interval { set; get; }

        /// <summary>
        /// Invoked when the timer elapses the interval.
        /// </summary>
        event Action? Elapsed;
    }
}
