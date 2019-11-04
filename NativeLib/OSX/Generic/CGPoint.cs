using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        [MarshalAs(UnmanagedType.SysUInt)]
        public Single X;
        [MarshalAs(UnmanagedType.SysUInt)]
        public Single Y;

        public CGPoint(Single x, Single y)
        {
            X = x;
            Y = y;
        }
    }
}