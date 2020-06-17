using System;
using System.Runtime.InteropServices;
using NativeLib.OSX.Generic;
using NativeLib.OSX.Input;

namespace NativeLib.OSX
{
    using CGEventRef = IntPtr;
    using CGDirectDisplayID = UInt32;
    using CGEventSourceRef = IntPtr;
    using io_service_t = UInt32;
    using CFAllocatorRef = IntPtr;
    using CGError = Int32;
    using mach_port_t = UInt32;
    using io_registry_entry_t = UInt32;
    using io_object_t = UInt32;
    using kern_return_t = Int32;
    using CFUUIDRef = IntPtr;
    using io_name_t = String;
    using io_string_t = String;
    using SInt32 = Int32;
    using UInt8 = Byte;

    public static class OSX
    {
        private const string Quartz = "/System/Library/Frameworks/Quartz.framework/Versions/Current/Quartz";
        private const string Foundation = "/System/Library/Frameworks/Foundation.framework/Foundation";
        private const string IOKit = "/System/Library/Frameworks/IOKit.framework/IOKit";

        public const string kIOServicePlane = "IOService";
        public const string kIO = "9DC7B780-9EC0-11D4-A54F-000A27052861";

        public static readonly IntPtr kIOCFPlugInInterfaceID = CFUUIDGetConstantUUIDWithBytes(IntPtr.Zero,
            0xC2, 0x44, 0xE8, 0x58, 0x10, 0x9C, 0x11, 0xD4,
            0x91, 0xD4, 0x00, 0x50, 0xE4, 0xC6, 0x42, 0x6F);

        public static readonly IntPtr kIOUSBDeviceUserClientTypeID = CFUUIDGetConstantUUIDWithBytes(IntPtr.Zero,
            0x9d, 0xc7, 0xb7, 0x80, 0x9e, 0xc0, 0x11, 0xD4,
            0xa5, 0x4f, 0x00, 0x0a, 0x27, 0x05, 0x28, 0x61);

        public static readonly IntPtr kIOUSBDeviceInterfaceID = CFUUIDGetConstantUUIDWithBytes(IntPtr.Zero,
            0x5c, 0x81, 0x87, 0xd0, 0x9e, 0xf3, 0x11, 0xD4,
             0x8b, 0x45, 0x00, 0x0a, 0x27, 0x05, 0x28, 0x61);

        [DllImport(Foundation)]
        public extern static CFUUIDRef CFUUIDGetConstantUUIDWithBytes(CFAllocatorRef alloc,
            UInt8 byte0, UInt8 byte1, UInt8 byte2, UInt8 byte3, UInt8 byte4, UInt8 byte5, UInt8 byte6, UInt8 byte7,
            UInt8 byte8, UInt8 byte9, UInt8 byte10, UInt8 byte11, UInt8 byte12, UInt8 byte13, UInt8 byte14, UInt8 byte15);

        [DllImport(Foundation)]
        public extern static CFUUIDBytes CFUUIDGetUUIDBytes(CFUUIDRef uuid);

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
        public extern static ulong CGDisplayPixelsWide(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static ulong CGDisplayPixelsHigh(CGDirectDisplayID display);

        [DllImport(Quartz)]
        public extern static CGError CGGetActiveDisplayList(UInt32 maxDisplays,
            [In, Out] CGDirectDisplayID[] activeDisplays, ref UInt32 displayCount);

        [DllImport(Quartz)]
        public extern static CGrect CGDisplayBounds(CGDirectDisplayID maxDisplays);

        [DllImport(IOKit)]
        public extern static io_registry_entry_t IORegistryEntryFromPath(mach_port_t masterPort,
            [MarshalAs(UnmanagedType.LPStr)] io_string_t path);

        [DllImport(IOKit)]
        public extern static kern_return_t IORegistryEntryGetParentEntry(io_registry_entry_t entry,
             [MarshalAs(UnmanagedType.LPStr)] io_name_t plane, ref io_registry_entry_t parent);

        [DllImport(IOKit)]
         public extern static unsafe kern_return_t IOCreatePlugInInterfaceForService(io_service_t service,
            CFUUIDRef pluginType, CFUUIDRef interfaceType, ref IUnknownCGuts** theInterface, ref SInt32 theScore);

        [DllImport(IOKit)]
        public extern static kern_return_t IOObjectRelease(io_object_t obj);

    }
}
