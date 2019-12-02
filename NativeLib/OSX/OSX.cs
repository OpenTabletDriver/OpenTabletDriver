using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    using CGEvent = IntPtr;
    using CGDirectDisplayID = UInt32;

    public static class OSX
    {
        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";

        [DllImport(Foundation)]
        public static extern void CFRelease(IntPtr handle);

        [DllImport(Quartz)]
        public extern static CGEvent CGEventCreate();

        [DllImport(Quartz)]
        public extern static CGPoint CGEventGetLocation(ref CGEvent eventRef);

        [DllImport(Quartz)]
        public extern static CGEvent CGWarpMouseCursorPosition(CGPoint newCursorPosition);

        [DllImport(Quartz)]
        public extern static CGEvent CGEventCreateMouseEvent(ref CGEvent source, CGEventType mouseType,
            CGPoint mouseCursorPosition, CGMouseButton mouseButton);
        
        [DllImport(Quartz)]
        public extern static CGEvent CGEventPost(ref CGEvent source, CGEventType mouseType, CGPoint mouseCursorPosition, CGMouseButton mouseButton);

        [DllImport(Quartz)]
        public extern static CGDirectDisplayID CGMainDisplayID();

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsWide(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsHigh(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static CGPoint CGPointMake(float x, float y);
    }
}