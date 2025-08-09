using System;

namespace OpenTabletDriver.Native.Windows.USB
{
    [Flags]
    public enum DIGCF
    {
        None = 0,
        Default = 1,
        Present = 2,
        AllClasses = 4,
        Profile = 8,
        DeviceInterface = 16
    }
}
