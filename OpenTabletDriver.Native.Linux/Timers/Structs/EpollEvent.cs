using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EpollEvent
    {
        [FieldOffset(0)] public uint events;
        [FieldOffset(4)] public int fd;
    }
}
