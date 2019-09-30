using System;
using System.Runtime.InteropServices;

namespace TabletDriverLib.Tools.Native
{
    internal class MacOSX
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CGPoint
        {
            [MarshalAs(UnmanagedType.SysUInt)]
            public Single X;
            [MarshalAs(UnmanagedType.SysUInt)]
            public Single Y;
        };

        [DllImport("/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz")]
        public extern static uint CGWarpMouseCursorPosition(CGPoint newCursorPosition);
    }
}