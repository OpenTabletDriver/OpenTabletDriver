using System;
using System.Runtime.InteropServices;

namespace NativeLib.OSX.Generic
{
    using UInt8 = Byte;

    [StructLayout(LayoutKind.Sequential)]
    public struct CFUUIDBytes
    {
        UInt8 byte0;
        UInt8 byte1;
        UInt8 byte2;
        UInt8 byte3;
        UInt8 byte4;
        UInt8 byte5;
        UInt8 byte6;
        UInt8 byte7;
        UInt8 byte8;
        UInt8 byte9;
        UInt8 byte10;
        UInt8 byte11;
        UInt8 byte12;
        UInt8 byte13;
        UInt8 byte14;
        UInt8 byte15;
	}
}
