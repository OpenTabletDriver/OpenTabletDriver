using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX.ApplicationServices
{
    public static class ApplicationServices
    {
        private const string libas = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";
        [DllImport(libas, EntryPoint = "AXIsProcessTrusted")]
        public static extern bool AXIsProcessTrusted();
    }
}

