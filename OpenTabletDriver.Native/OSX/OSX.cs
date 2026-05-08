using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Native.OSX.Input;

namespace OpenTabletDriver.Native.OSX
{
    using static ObjectiveCRuntime;
    using CGDirectDisplayID = UInt32;
    using CGError = Int32;
    using CGEventRef = IntPtr;
    using CGEventSourceRef = IntPtr;

    public static class OSX
    {
        public const int CGEventSourceStateHIDSystemState = 1;
        public const int CGEventSourceStatePrivate = -1;

        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";
        private const string AppKit = "/System/Library/Frameworks/AppKit.framework/AppKit";

        static OSX()
        {
            LibSystem.dlopen(AppKit, 0);
        }


        [DllImport(Foundation)]
        public static extern void CFRelease(IntPtr handle);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventCreate(CGEventSourceRef source);

        [DllImport(Quartz)]
        public static extern CGPoint CGEventGetLocation(CGEventRef eventRef);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventCreateMouseEvent(CGEventSourceRef source, CGEventType mouseType,
            CGPoint mouseCursorPosition, CGMouseButton mouseButton);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventCreateKeyboardEvent(CGEventSourceRef source, CGKeyCode virtualKey, bool keyDown);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventSetType(CGEventRef eventRef, CGEventType type);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventSetIntegerValueField(CGEventRef eventRef, CGEventField field, long value);

        [DllImport(Quartz)]
        public static extern void CGEventSetDoubleValueField(CGEventRef eventRef, CGEventField field, double value);

        [DllImport(Quartz)]
        public static extern void CGEventSetLocation(CGEventRef eventRef, CGPoint location);

        [DllImport(Quartz)]
        public static extern CGEventRef CGEventCreateScrollWheelEvent2(CGEventRef eventRef, CGScrollEventUnit units, uint wheelCount, int wheel1, int wheel2, int wheel3);

        [DllImport(Quartz)]
        public static extern void CGEventSetFlags(CGEventRef eventRef, ulong flags);

        [DllImport(Quartz)]
        public static extern CGEventSourceRef CGEventSourceCreate(int stateID);

        [DllImport(Quartz)]
        public static extern ulong CGEventSourceFlagsState(int stateID);

        [DllImport(Quartz, EntryPoint = "CGEventPost")]
        private static extern void _CGEventPost(CGEventTapLocation tap, CGEventRef eventRef);


        [DllImport(Quartz)]
        public static extern CGError CGGetActiveDisplayList(uint maxDisplays,
            [In, Out] CGDirectDisplayID[] activeDisplays, out uint displayCount);

        [DllImport(Quartz)]
        public static extern CGRect CGDisplayBounds(CGDirectDisplayID displayID);

        public static double GetDoubleClickInterval()
        {
            return objc_msgSend_double(objc_getClass("NSEvent"), sel_registerName("doubleClickInterval"));
        }

        public static void CGEventPost(CGEventTapLocation tap, CGEventRef eventRef)
        {
            var pool = objc_autoreleasePoolPush();
            _CGEventPost(tap, eventRef);
            objc_autoreleasePoolPop(pool);
        }
    }
}
