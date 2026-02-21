using System.Runtime.InteropServices;

// TODO: remove nullable disable
#nullable disable

namespace OpenTabletDriver.Native.OSX.Generic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGRect
    {
        public CGPoint origin;
        public CGSize size;

        public CGRect(CGPoint origin, CGSize size)
        {
            this.origin = origin;
            this.size = size;
        }
    }
}
