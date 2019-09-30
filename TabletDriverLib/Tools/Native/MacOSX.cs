using System;
using System.Runtime.InteropServices;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Native
{
    internal class MacOSX
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CGPoint
        {
            [MarshalAs(UnmanagedType.SysUInt)]
            public Single X;
            [MarshalAs(UnmanagedType.SysUInt)]
            public Single Y;

            public static explicit operator CGPoint(Point point)
            {
                return new CGPoint
                {
                    X = (Single)point.X,
                    Y = (Single)point.Y,
                };
            }

            public static explicit operator Point(CGPoint point)
            {
                return new Point(point.X, point.Y);
            }
        };

        [DllImport("/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz")]
        public extern static uint CGWarpMouseCursorPosition(CGPoint newCursorPosition);
    }
}