using System;

namespace OpenTabletDriver.Native.OSX.Input
{
    public delegate IntPtr CGEventTapCallback
    (
        IntPtr proxy,
        CGEventType type,
        IntPtr @event,
        IntPtr refcon
    );
}
