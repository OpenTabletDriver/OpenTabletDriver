using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Evdev.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct input_absinfo
    {
        public int value;
        public int minimum;
        public int maximum;
        public int fuzz;
        public int flat;
        public int resolution;
    }
}
