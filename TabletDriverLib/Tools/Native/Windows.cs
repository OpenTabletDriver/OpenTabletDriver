using System.Runtime.InteropServices;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Native
{
    internal class Windows
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static explicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }

            public static explicit operator POINT(Point p)
            {
                return new POINT((int)p.X, (int)p.Y);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);
    }
}