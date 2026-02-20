using System;

namespace OpenTabletDriver.Native.Windows.Timers
{
    [Flags]
    public enum EventType : uint
    {
        /// <summary>
        /// Event occurs once, after uDelay milliseconds.
        /// </summary>
        TIME_ONESHOT = 0,
        /// <summary>
        /// Event occurs every after uDelay milliseconds.
        /// </summary>
        TIME_PERIODIC = 1,
        /// <summary>
        /// Immediately stop timer when requested.
        /// </summary>
        TIME_KILL_SYNCHRONOUS = 0x100,
    }
}
