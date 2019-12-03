using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public Single x;
        public Single y;

        public CGPoint(Single x, Single y)
        {
            this.x = x;
            this.y = y;
        }
    }
}