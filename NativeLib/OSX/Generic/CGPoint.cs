using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        //TODO: revert to float on 32 bit binary.
        public Double x;
        public Double y;

        public CGPoint(Double x, Double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
