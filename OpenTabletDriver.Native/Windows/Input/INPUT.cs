using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct INPUT
    {
        public INPUT_TYPE type;
#if OTDPLATFORM32
        public fixed byte data[24];
#else
        private fixed byte padding[4];
        public fixed byte data[32];
#endif
        public MOUSEINPUT* MouseInputPtr => (MOUSEINPUT*)Unsafe.AsPointer(ref data[0]);
        public KEYBDINPUT* KeyboardInputPtr => (KEYBDINPUT*)Unsafe.AsPointer(ref data[0]);
        public HARDWAREINPUT* HardwareInputPtr => (HARDWAREINPUT*)Unsafe.AsPointer(ref data[0]);
    }

    public enum INPUT_TYPE : uint
    {
        MOUSE_INPUT,
        KEYBD_INPUT,
        HARDWARE_INPUT
    }
}