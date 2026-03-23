using System;

namespace OpenTabletDriver.Native.MacOS.Input
{
    [Flags]
    public enum CGEventFlags : ulong
    {
        kCGEventFlagMaskAlphaShift = 0x00010000,
        kCGEventFlagMaskShift = 0x00020000,
        kCGEventFlagMaskControl = 0x00040000,
        kCGEventFlagMaskAlternate = 0x00080000,
        kCGEventFlagMaskCommand = 0x00100000,
        kCGEventFlagMaskHelp = 0x00400000,
        kCGEventFlagMaskSecondaryFn = 0x00800000,
        kCGEventFlagMaskNumericPad = 0x00200000
    }
}
