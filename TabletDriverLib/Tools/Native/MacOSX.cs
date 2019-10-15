using System;
using System.Runtime.InteropServices;
using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Native
{
    using CGEvent = IntPtr;
    using CGDirectDisplayID = UInt32;

    internal class MacOSX
    {
        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";

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
                    X = point.X,
                    Y = point.Y,
                };
            }

            public static explicit operator Point(CGPoint point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport(Foundation)]
        public static extern void CFRelease(IntPtr handle);

        #region Cursor

        [DllImport(Quartz)]
        public extern static CGEvent CGEventCreate();

        [DllImport(Quartz)]
        public extern static CGPoint CGEventGetLocation(ref CGEvent eventRef);

        [DllImport(Quartz)]
        public extern static CGEvent CGWarpMouseCursorPosition(CGPoint newCursorPosition);
        
        #endregion

        #region Display

        [DllImport(Quartz)]
        public extern static CGDirectDisplayID CGMainDisplayID();

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsWide(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsHigh(CGDirectDisplayID display);

        #endregion
    }
}