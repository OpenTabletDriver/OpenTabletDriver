using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.MacOS.IOkit
{
    public static class IOKit
    {
        private const string libiokiot = "/System/Library/Frameworks/IOKit.framework/IOKit";

        [DllImport(libiokiot)]
        public static extern IOHIDAccessType IOHIDCheckAccess(IOHIDRequestType requestType);

        [DllImport(libiokiot)]
        public static extern bool IOHIDRequestAccess(IOHIDRequestType requestType);
    }
}
