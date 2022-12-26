using System;

namespace OpenTabletDriver.Native.MacOS.Timers
{
    [Flags]
    public enum FilterFlags : UInt32
    {
        NOTE_MSECONDS = 0x00000000,
        NOTE_SECONDS = 0x00000001,
        NOTE_USECONDS = 0x00000002,
        NOTE_NSECONDS = 0x00000004,
        NOTE_ABSOLUTE = 0x00000008,
        NOTE_LEEWAY = 0x00000010,
        NOTE_CRITICAL = 0x00000020,
        NOTE_BACKGROUND = 0x00000040,
        NOTE_MACH_CONTINUOUS_TIME = 0x00000080,
        NOTE_MACHTIME = 0x00000100
    }
}
