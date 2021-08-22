using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct INPUT
    {
        public INPUT_TYPE type;
        public fixed byte data[28];

        public static MOUSEINPUT* GetMouseInputPtr(INPUT* input)
        {
            return (MOUSEINPUT*)input->data;
        }

        public static KEYBDINPUT* GetKeyboardInputPtr(INPUT* input)
        {
            return (KEYBDINPUT*)input->data;
        }

        public static HARDWAREINPUT* GetHardwareInputPtr(INPUT* input)
        {
            return (HARDWAREINPUT*)input->data;
        }

        public static int Size => Unsafe.SizeOf<INPUT>();
    }

    public enum INPUT_TYPE
    {
        MOUSE_INPUT,
        KEYBD_INPUT,
        HARDWARE_INPUT
    }
}