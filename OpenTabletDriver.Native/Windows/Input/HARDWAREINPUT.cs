using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }
}
