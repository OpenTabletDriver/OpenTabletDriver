using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS
{
    public static class ObjectiveCRuntime
    {
        private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        public static extern double objc_msgSend_double(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLibrary, CharSet = CharSet.Ansi)]
        public static extern IntPtr sel_registerName(string name);

        [DllImport(ObjCLibrary, CharSet = CharSet.Ansi)]
        public static extern IntPtr objc_getClass(string namePtr);
    }
}

