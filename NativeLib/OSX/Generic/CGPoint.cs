using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public Single x;
        public Single y;
    }
}