using System;
namespace OpenTabletDriver.Native.OSX.IOkit
{
    public enum IOHIDRequestType : uint
    {
        kIOHIDRequestTypePostEvent = 0,
        kIOHIDRequestTypeListenEvent = 1
    }
}
