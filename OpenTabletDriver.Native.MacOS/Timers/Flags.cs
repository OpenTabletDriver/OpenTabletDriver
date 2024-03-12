using System;

namespace OpenTabletDriver.Native.MacOS.Timers
{
    [Flags]
    public enum Flags : UInt16
    {
        EV_ADD = 0x0001,
        EV_DELETE = 0x0002,
        EV_ENABLE = 0x0004,
        EV_DISABLE = 0x0008,
        EV_ONESHOT = 0x0010,
        EV_CLEAR = 0x0020,
        EV_RECEIPT = 0x0040,
        EV_DISPATCH = 0x0080,
        EV_UDATA_SPECIFIC = 0x0100,
        EV_DISPATCH2 = EV_DISPATCH | EV_UDATA_SPECIFIC,
        EV_VANISHED = 0x0200,
        EV_SYSFLAGS = 0xF000,
        EV_FLAG0 = 0x1000,
        EV_FLAG1 = 0x2000,
        EV_EOF = 0x8000,
        EV_ERROR = 0x4000
    }
}
