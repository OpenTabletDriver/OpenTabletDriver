﻿using System;
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
        public extern static CGEventRef CGEventCreate(CGEventSourceRef source);

        [DllImport(Quartz)]
        public extern static CGPoint CGEventGetLocation(CGEventRef eventRef);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreateMouseEvent(CGEventSourceRef source, CGEventType mouseType,
            CGPoint mouseCursorPosition, CGMouseButton mouseButton);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreateKeyboardEvent(CGEventSourceRef source, CGKeyCode virtualKey, bool keyDown);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventSetType(CGEventRef eventRef, CGEventType type);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventSetIntegerValueField(CGEventRef eventRef, CGEventField field, long value);

        [DllImport(Quartz)]
        public extern static void CGEventSetDoubleValueField(CGEventRef eventRef, CGEventField field, double value);

        [DllImport(Quartz)]
        public extern static void CGEventSetLocation(CGEventRef eventRef, CGPoint location);

        [DllImport(Quartz)]
        public extern static void CGEventSetFlags(CGEventRef eventRef, ulong flags);

        [DllImport(Quartz)]
        public extern static CGEventSourceRef CGEventSourceCreate(int stateID);

        [DllImport(Quartz)]
        public extern static ulong CGEventSourceFlagsState(int stateID);

        [DllImport(Quartz, EntryPoint = "CGEventPost")]
        private extern static void _CGEventPost(CGEventTapLocation tap, CGEventRef eventRef);


        [DllImport(Quartz)]
        public extern static CGError CGGetActiveDisplayList(uint maxDisplays,
            [In, Out] CGDirectDisplayID[] activeDisplays, out uint displayCount);

        [DllImport(Quartz)]
        public extern static CGRect CGDisplayBounds(CGDirectDisplayID displayID);

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
