using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT(int x, int y)
    {
        public int X = x;
        public int Y = y;
    }
}
