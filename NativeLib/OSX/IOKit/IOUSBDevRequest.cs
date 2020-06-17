using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX
{
    using UInt8 = Byte;
    using UInt16 = UInt16;

    [StructLayout(LayoutKind.Sequential)]
    public struct IOUSBDevRequest
    {
        public UInt8 bmRequestType;
        public UInt8 bRequest;
        public UInt16 wValue;
        public UInt16 wIndex;
        public UInt16 wLength;
        public IntPtr pData;
        public UInt32 wLenDone;
    }
}
