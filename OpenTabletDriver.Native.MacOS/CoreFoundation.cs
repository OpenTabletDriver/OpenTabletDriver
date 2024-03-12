using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS
{
    static public class CoreFoundation
    {
        private const string CFLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private static IntPtr handle = LibSystem.dlopen(CFLib, 0);

        public static IntPtr kCFBooleanTrue = LibSystem.GetConstant(handle, "kCFBooleanTrue");
        public static IntPtr kCFBooleanFalse = LibSystem.GetConstant(handle, "kCFBooleanFalse");

        [DllImport(CFLib)]
        public static extern IntPtr CFDictionaryCreateMutable(IntPtr allocator, long capacity, IntPtr keyCallBacks, IntPtr valueCallBacks);

        [DllImport(CFLib)]
        public static extern void CFDictionaryAddValue(IntPtr theDict, IntPtr key, IntPtr value);

        [DllImport(CFLib)]
        public static extern void CFRelease(IntPtr cf);
    }
}
