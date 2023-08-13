using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS
{
    public static class LibSystem
    {
        private const string SysLib = "/usr/lib/libSystem.dylib";

        [DllImport(SysLib, CharSet = CharSet.Ansi)]
        public static extern IntPtr dlopen(string path, int mode);

        [DllImport(SysLib, CharSet = CharSet.Ansi)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        public static IntPtr GetConstant(IntPtr handle, string symbol)
        {
            IntPtr ptr = dlsym(handle, symbol);
            return Marshal.PtrToStructure<IntPtr>(ptr);
        }
    }
}
