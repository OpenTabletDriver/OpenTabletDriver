using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.Input
{
    using ScanCodeShort = Int16;
    using VirtualKeyShort = Int16;

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public VirtualKeyShort wVk;
        public ScanCodeShort wScan;
        public KEYEVENTF dwFlags;
        public int time;
        public UIntPtr dwExtraInfo;
    }
}
