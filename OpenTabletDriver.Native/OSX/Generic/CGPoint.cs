using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX
{
    [StructLayout(LayoutKind.Sequential)]
    public record struct CGPoint(double x, double y)
    {
        public double x = x;
        public double y = y;

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
