using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGSize
    {
        //TODO: revert to float on 32 bit binary.
        public Double width;
        public Double height;

        public CGSize(Double width, Double height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
