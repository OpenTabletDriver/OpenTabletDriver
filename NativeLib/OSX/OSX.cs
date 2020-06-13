using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    using CGEventRef = IntPtr;
    using CGDirectDisplayID = UInt32;
    using CGEventSourceRef = IntPtr;

    public static class OSX
    {
        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";

        [DllImport(Foundation)]
        public static extern void CFRelease(IntPtr handle);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreate(CGEventSourceRef source);

        [DllImport(Quartz)]
        public extern static CGPoint CGEventGetLocation(CGEventRef eventRef);

        [DllImport(Quartz)]
        public extern static CGEventRef CGWarpMouseCursorPosition(CGPoint newCursorPosition);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreateMouseEvent(CGEventSourceRef source, CGEventType mouseType,
            CGPoint mouseCursorPosition, CGMouseButton mouseButton);
        
        [DllImport(Quartz)]
        public extern static CGEventRef CGEventPost(CGEventTapLocation tap, CGEventRef eventRef);

        [DllImport(Quartz)]
        public extern static CGDirectDisplayID CGMainDisplayID();

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsWide(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsHigh(CGDirectDisplayID display);
    }
}