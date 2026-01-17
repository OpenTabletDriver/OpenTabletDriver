using System;

namespace OpenTabletDriver.Native.OSX.Input
{
    public delegate IntPtr CGEventTapCallback
    (
        IntPtr proxy,
        CGEventTypeMask type,
        IntPtr @event,
        IntPtr refcon
    );
}
