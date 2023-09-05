using System;

namespace OpenTabletDriver.Tools.udev
{
    [Flags]
    public enum UdevDeviceFlags
    {
        None = 0,
        LibInputOverride = 1 << 0
    }
}
