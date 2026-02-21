using System.Runtime.InteropServices;

// TODO: remove nullable disable
#nullable disable

namespace OpenTabletDriver.Native.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public double x;
        public double y;

        public CGPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static CGPoint operator +(CGPoint a, CGPoint b)
        {
            return new CGPoint(a.x + b.x, a.y + b.y);
        }

        public static CGPoint operator -(CGPoint a, CGPoint b)
        {
            return new CGPoint(a.x - b.x, a.y - b.y);
        }
    }
}
