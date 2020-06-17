using System;
using System.Runtime.InteropServices;
using NativeLib.OSX.Generic;

namespace NativeLib.OSX
{
    using static Utility;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe public delegate UInt32 QueryInterface(IUnknownCGuts** thisPointer, CFUUIDBytes iid, ref IntPtr ppv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 Release(IntPtr thisPointer);

    [StructLayout(LayoutKind.Sequential)]
    public struct IUnknownCGuts 
    {
        IntPtr reserved;
        IntPtr QueryInterfacePtr;
        IntPtr AddRefPtr;
        IntPtr ReleasePtr;

        public QueryInterface QueryInterface => Wrap<QueryInterface>(QueryInterfacePtr);
        public Release Release => Wrap<Release>(ReleasePtr);
    }
}