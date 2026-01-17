using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX
{
    using CFMachPortRef = IntPtr;
    using CFRunLoopMode = IntPtr;
    using CFRunLoopRef = IntPtr;
    using CFRunLoopSourceRef = IntPtr;

    static public class CoreFoundation
    {

        private const string CFLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private static IntPtr handle = LibSystem.dlopen(CFLib, 0);

        public static IntPtr kCFBooleanTrue = LibSystem.GetConstant(handle, "kCFBooleanTrue");
        public static IntPtr kCFBooleanFalse = LibSystem.GetConstant(handle, "kCFBooleanFalse");
        public static CFRunLoopMode kCFRunLoopCommonModes = LibSystem.GetConstant(handle, "kCFRunLoopCommonModes");

        [DllImport(CFLib)]
        public static extern CFRunLoopSourceRef CFMachPortCreateRunLoopSource(IntPtr allocator, CFMachPortRef port, int order);

        [DllImport(CFLib)]
        public static extern CFRunLoopRef CFRunLoopGetCurrent();

        [DllImport(CFLib)]
        public static extern void CFRunLoopRun();

        [DllImport(CFLib)]
        public static extern void CFRunLoopStop(CFRunLoopRef runLoop);

        [DllImport(CFLib)]
        public static extern void CFRunLoopAddSource(CFRunLoopRef runLoop, CFRunLoopSourceRef source, CFRunLoopMode mode);

        [DllImport(CFLib)]
        public static extern IntPtr CFDictionaryCreateMutable(IntPtr allocator, long capacity, IntPtr keyCallBacks, IntPtr valueCallBacks);

        [DllImport(CFLib)]
        public static extern void CFDictionaryAddValue(IntPtr theDict, IntPtr key, IntPtr value);
    }
}
