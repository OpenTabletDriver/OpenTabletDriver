using System;

namespace OpenTabletDriver.Native.Windows.Timers
{
    [Flags]
    public enum EventType : uint
    {
        TIME_ONESHOT = 0,      //Event occurs once, after uDelay milliseconds.
        TIME_PERIODIC = 1,
    }
}