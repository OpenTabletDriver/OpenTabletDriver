using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A high precision event timer stopwatch.
    /// </summary>
    [PublicAPI]
    public sealed class HPETDeltaStopwatch
    {
        private static readonly Stopwatch InternalWatch = Stopwatch.StartNew();
        private TimeSpan _start;
        private TimeSpan _end;
        private bool _isRunning;

        public HPETDeltaStopwatch(bool startRunning = true)
        {
            _isRunning = startRunning;
            _start = _isRunning ? InternalWatch.Elapsed : default;
        }

        /// <summary>
        /// The amount of time since the last reset.
        /// </summary>
        public static TimeSpan RuntimeElapsed => InternalWatch.Elapsed;

        /// <summary>
        /// The amount of time since the last start.
        /// </summary>
        public TimeSpan Elapsed => _isRunning ? RuntimeElapsed - _start : _end - _start;

        /// <summary>
        /// Starts the stopwatch.
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _start = InternalWatch.Elapsed;
            }
        }

        /// <summary>
        /// Stops the stopwatch.
        /// </summary>
        /// <returns>
        /// Elapsed time since the last start.
        /// </returns>
        public TimeSpan Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _end = InternalWatch.Elapsed;
            }
            return _end - _start;
        }

        /// <summary>
        /// Restarts the stopwatch.
        /// </summary>
        /// <returns>
        /// Elapsed time since the last start.
        /// </returns>
        public TimeSpan Restart()
        {
            if (_isRunning)
            {
                var current = InternalWatch.Elapsed;
                var delta = current - _start;
                _start = current;
                return delta;
            }
            else
            {
                var delta = _end - _start;
                Start();
                return delta;
            }
        }

        /// <summary>
        /// Stops and resets the stopwatch.
        /// </summary>
        /// <returns>
        /// Elapsed time since the last start.
        /// </returns>
        public TimeSpan Reset()
        {
            var delta = Stop();
            _start = _end = default;
            return delta;
        }
    }
}
