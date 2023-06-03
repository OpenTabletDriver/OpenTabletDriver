using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX.IOkit
{
    public static class IOKit
    {
        private const string libiokiot = "/System/Library/Frameworks/IOKit.framework/IOKit";

        [DllImport(libiokiot, EntryPoint = "IOHIDCheckAccess")]
        public static extern IOHIDAccessType IOHIDCheckAccess(IOHIDRequestType requestType);
    }
}
