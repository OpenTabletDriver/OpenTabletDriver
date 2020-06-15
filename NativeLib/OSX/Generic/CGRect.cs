using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGrect
    {
        public CGPoint origin;
        public CGSize size;

        public CGrect(CGPoint origin, CGSize size)
        {
            this.origin = origin;
            this.size = size;
        }
    }

}
