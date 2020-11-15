using System;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.OSX.Generic;
using OpenTabletDriver.Native.OSX.Input;

namespace OpenTabletDriver.Native.OSX
{
    using CGEventRef = IntPtr;
    using CGDirectDisplayID = UInt32;
    using CGEventSourceRef = IntPtr;
    using CGError = Int32;

    public static class OSX
    {
        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";
        private const string LibDispatch = "/usr/lib/system/libdispatch.dylib";

        public delegate void TimerCallback();

        [DllImport(Foundation)]
        public static extern void CFRelease(IntPtr handle);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreate(CGEventSourceRef source);

        [DllImport(Quartz)]
        public extern static CGPoint CGEventGetLocation(CGEventRef eventRef);

        [DllImport(Quartz)]
        public extern static CGEventRef CGWarpMouseCursorPosition(CGPoint newCursorPosition);

        [DllImport(Quartz)]
        public extern static CGError CGAssociateMouseAndMouseCursorPosition(int connected);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreateMouseEvent(CGEventSourceRef source, CGEventType mouseType,
            CGPoint mouseCursorPosition, CGMouseButton mouseButton);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventCreateKeyboardEvent(CGEventSourceRef source, CGKeyCode virtualKey, bool keyDown);

        [DllImport(Quartz)]
        public extern static CGEventRef CGEventPost(CGEventTapLocation tap, CGEventRef eventRef);

        [DllImport(Quartz)]
        public extern static CGDirectDisplayID CGMainDisplayID();

        [DllImport(Quartz)]
        public extern static CGError CGGetActiveDisplayList(uint maxDisplays,
            [In, Out] CGDirectDisplayID[] activeDisplays, out uint displayCount);

        [DllImport(Quartz)]
        public extern static CGRect CGDisplayBounds(CGDirectDisplayID displayID);

        [DllImport(LibDispatch, EntryPoint = "dispatch_queue_create")]
        public extern static IntPtr DispatchQueueCreate(string label, IntPtr attr);

        [DllImport(LibDispatch, EntryPoint = "dispatch_source_create")]
        public extern static IntPtr DispatchSourceCreate(IntPtr type, UIntPtr handle, UIntPtr mask, IntPtr queue);

        [DllImport(LibDispatch, EntryPoint = "dispatch_source_set_event_handler")]
        public extern static void DispatchSourceSetEventHandler(IntPtr source, TimerCallback handler);

        [DllImport(LibDispatch, EntryPoint = "dispatch_resume")]
        public extern static void DispatchResume(IntPtr dispatchObject);

        [DllImport(LibDispatch, EntryPoint = "dispatch_source_cancel")]
        public extern static void DispatchSourceCancel(IntPtr source);

        [DllImport(LibDispatch, EntryPoint = "dispatch_release")]
        public extern static void DispatchRelease(IntPtr dispatchObject);

        [DllImport(LibDispatch, EntryPoint = "dispatch_time")]
        public extern static ulong DispatchTime(ulong when, long delta);

        [DllImport(LibDispatch, EntryPoint = "dispatch_source_set_timer")]
        public extern static void DispatchSourceSetTimer(IntPtr source, ulong start, ulong interval, ulong leeway);
    }
}