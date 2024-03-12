using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS.ApplicationServices
{
    public static class ApplicationServices
    {
        private const string libas = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

        private static IntPtr handle = LibSystem.dlopen(libas, 0);

        public static IntPtr kAXTrustedCheckOptionPrompt = LibSystem.GetConstant(handle, "kAXTrustedCheckOptionPrompt");

        [DllImport(libas)]
        public static extern bool AXIsProcessTrusted();

        [DllImport(libas)]
        public static extern bool AXIsProcessTrustedWithOptions(IntPtr options);
    }
}
